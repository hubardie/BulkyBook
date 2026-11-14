using BulkyBook.Business.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;

        public ShoppingCartController(IShoppingCartService shoppingCartService, IApplicationUserService applicationUserService, IOrderService orderService, IEmailService emailService)
        {
            _shoppingCartService = shoppingCartService;
            _applicationUserService = applicationUserService;
            _orderService = orderService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = await _shoppingCartService.GetUserShoppingCartItemsAsync(userId);
            var user = await _applicationUserService.GetUserByIdAsync(userId);

            ShoppingCartVM cartVM = new()
            {
                ShoppingCartList = cartItems,
                OrderHeader = new()
                {
                    ApplicationUser = user,
                    ApplicationUserId = userId,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode
                }
            };
            foreach (var cart in cartVM.ShoppingCartList)
            {
                cartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(cartVM);
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST(ShoppingCartVM shopppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = await _shoppingCartService.GetUserShoppingCartItemsAsync(userId);

            shopppingCartVM.ShoppingCartList = cartItems;
            shopppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
            shopppingCartVM.OrderHeader.ApplicationUserId = userId;

            foreach (var cart in shopppingCartVM.ShoppingCartList)
            {
                shopppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            shopppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            shopppingCartVM.OrderHeader.OrderDetails = shopppingCartVM.ShoppingCartList.Select(cart => new OrderDetails
            {
                ProductId = cart.ProductId,
                Price = cart.Price,
                Count = cart.Count,
                Product = cart.Product
            }).ToList();
            // Create order
            await _orderService.CreateOrderAsync(shopppingCartVM.OrderHeader);

            try
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + "Customer/ShoppingCart/OrderConfirmation?orderHeaderId=" + shopppingCartVM.OrderHeader.Id,
                    CancelUrl = domain + "Customer/ShoppingCart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", shopppingCartVM.OrderHeader.Id.ToString() }
                    }
                };

                foreach (var item in shopppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Stripe expects amount in cents
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }

                        },
                        Quantity = item.Count,

                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                await _orderService.UpdateStripePaymentAsync(shopppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);

                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303); // after Post endpoint to prevent resubmissions

            }
            catch (Exception ex)
            {
                TempData["error"] = "Payment processing failed. Please try again."                    ;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OrderConfirmation(int orderHeaderId)
        {
            var orderHeader = await _orderService.GetOrderByIdAsync(orderHeaderId, includeUser: true);
            if (orderHeader == null)
            {
                return NotFound();
            }
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            if (orderHeader.ApplicationUserId != userId) // Check if the loged user is the same as the user who placed the order
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }
            try 
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    await _orderService.UpdateStripePaymentAsync(orderHeaderId, session.Id, session.PaymentIntentId);
                    await _orderService.UpdateOrderStatusAsync(orderHeaderId, SD.StatusApproved);
                    TempData["success"] = "Payment completed successfully. Your order has been confirmed.";
                }
                else
                {
                    TempData["warning"] = "Payment status is pending. Please contact support if you completed the payment.";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = "Unable to verify payment status. Please contact support with your order number.";
            }
            var user = await _applicationUserService.GetUserByIdAsync(userId);
            await _emailService.SendOrderConfirmationEmailAsync(user.Email, orderHeader.Id, (decimal)orderHeader.OrderTotal);

            return View(orderHeaderId);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart != null)
            {
                if (cart.Count < 1000)
                {
                    cart.Count++;
                    await _shoppingCartService.UpdateCartAsync(cart);
                    await UpdateCartSessionAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart != null && cart?.Count > 0)
            {
                cart.Count--;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count = 0;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UpdateCart(int cartId, int count)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }


            if (count <= 1)
            {
                cart.Count = 0;
            }
            else
            {
                if (count >= 1000)
                {
                    cart.Count = 1000;
                }
                else
                {
                    cart.Count = count;
                }
            }
            await _shoppingCartService.UpdateCartAsync(cart);
            await UpdateCartSessionAsync();
            return Ok(new { success = true });
        }

        private async Task UpdateCartSessionAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var count = await _shoppingCartService.GetCartCountAsync(userId);
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
        }

    }
}

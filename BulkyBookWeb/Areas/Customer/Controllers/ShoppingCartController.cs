using BulkyBook.Business.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


        public ShoppingCartController(IShoppingCartService shoppingCartService, IApplicationUserService applicationUserService, IOrderService orderService  )
        {
            _shoppingCartService = shoppingCartService;
            _applicationUserService = applicationUserService;
            _orderService = orderService;
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
            shopppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            shopppingCartVM.OrderHeader.OrderDetails = shopppingCartVM.ShoppingCartList.Select(cart => new OrderDetails
            {
                ProductId = cart.ProductId,
                Price = cart.Price,
                Count = cart.Count,
                Product = cart.Product
            });
            // Create order
            await _orderService.CreateOrderAsync(shopppingCartVM.OrderHeader);
            RedirectToAction(nameof(OrderConfirmation), new { id = shopppingCartVM.OrderHeader.Id});

        }

        public async Task<IActionResult> OrderConfirmation(int orderHeaderId)
        {
            return View(orderHeaderId);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart != null)
            {
                if(cart.Count < 1000)
                {
                    cart.Count++;
                    await _shoppingCartService.UpdateCartAsync(cart);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _shoppingCartService.GetShoppingCartByIdAsync(cartId);
            if (cart != null && cart?.Count>0)
            {
                cart.Count--;
                await _shoppingCartService.UpdateCartAsync(cart);
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
 
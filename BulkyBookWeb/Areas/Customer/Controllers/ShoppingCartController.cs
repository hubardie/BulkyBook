using BulkyBook.Business.IServices;
using BulkyBook.Models.ViewModels;
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


        public ShoppingCartController(IShoppingCartService shoppingCartService, IApplicationUserService applicationUserService)
        {
            _shoppingCartService = shoppingCartService;
            _applicationUserService = applicationUserService;
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
    }
}

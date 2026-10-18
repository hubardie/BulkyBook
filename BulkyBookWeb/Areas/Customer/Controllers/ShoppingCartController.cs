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
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;


        public ShoppingCartController(IProductService productService, IShoppingCartService shoppingCartService)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
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

            ShoppingCartVM cartVM = new()
            {
                ShoppingCartList = cartItems,
                OrderHeader = new()
            };
            foreach (var cart in cartVM.ShoppingCartList)
            {
                cartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            
            }
            return View(cartVM);
        }
    }
}

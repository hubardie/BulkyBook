using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart?> GetShoppingCartByIdAsync(int cartId);

        Task<IEnumerable<ShoppingCart>> GetUserShoppingCartItemsAsync(string userId);
        Task<int> GetCartCountAsync(string userId);
        Task<ShoppingCart> AddToCartAsync(ShoppingCart cart);
        Task UpdateCartAsync(ShoppingCart cart);
        Task ClearCartAsync(string userId);


    }
}

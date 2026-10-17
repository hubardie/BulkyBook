using BulkyBook.Business.IServices;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task ClearCartAsync(string userId)
        {
            var cartItems = await _context.ShoppingCarts.Where(u => u.ApplicationUserId == userId).ToListAsync();
            if (cartItems.Any())
            { 
                _context.ShoppingCarts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _context.ShoppingCarts.Where(u => u.ApplicationUserId == userId).SumAsync(c => c.Count);
        }

        public async Task<ShoppingCart?> GetShoppingCartByIdAsync(int cartId)
        {
            return await _context.ShoppingCarts.Include(p => p.Product).FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<IEnumerable<ShoppingCart>> GetUserShoppingCartItemsAsync(string userId)
        {
            return await _context.ShoppingCarts.Include(p => p.Product).Where(u => u.ApplicationUserId == userId).ToListAsync();
        }
        public Task<ShoppingCart> AddToCartAsync(ShoppingCart cart)
        {
            throw new NotImplementedException();
        }
        public Task UpdateCartAsync(ShoppingCart cart)
        {
            throw new NotImplementedException();
        }
    }
}

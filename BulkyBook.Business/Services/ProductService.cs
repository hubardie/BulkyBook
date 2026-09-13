using BulkyBook.Business.IServices;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public async Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategory = false)
        {
            var products = _context.Products.AsQueryable();
            if (includeCategory)
            {
                products = products.Include(p => p.Category);
            }
            return await products.ToListAsync();
        }
        public async Task<Product?> GetProductByIdASync(int id)
        {
            return await _context.Products.FindAsync(id); 
        }
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public  async Task<Product> CreateProductAsync(Product Product)
        {
            _context.Add(Product);
            await _context.SaveChangesAsync();
            return Product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var Product = await _context.Products.FindAsync(id);
            if (Product == null)
            {
                throw new KeyNotFoundException($"Product {id} not found");
            }
            _context.Remove(Product);
            await _context.SaveChangesAsync();  
        }

       

        public async Task UpdateProductAsync(Product Product)
        {
            _context.Products.Update(Product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsProductTitleUniqueAsync(string name, int? ProductId = null) 
        {
            if (ProductId.HasValue)
            {
                return !await _context.Products.AnyAsync(c => c.Title.ToLower() == name.ToLower() && c.Id != ProductId.Value);
            }
            else
            {
                return !await _context.Products.AnyAsync(c => c.Title.ToLower() == name.ToLower());
            }   

        }

    }

}

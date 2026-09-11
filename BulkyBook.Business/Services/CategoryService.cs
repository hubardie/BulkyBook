using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.IServices
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public async Task<IEnumerable<Category>> GetAllcategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<Category?> GetCategoryByIdASync(int id)
        {
            return await _context.Categories.FindAsync(id); 
        }
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public  async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category {id} not found");
            }
            _context.Remove(category);
            await _context.SaveChangesAsync();  
        }

       

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }

}

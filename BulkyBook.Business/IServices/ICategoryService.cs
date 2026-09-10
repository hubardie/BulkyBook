using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface ICategoryService
    { 
        Task<Category?> GetCategoryByIdASync(int id);
        Task<IEnumerable<Category>> GetAllcategoriesAsync();
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);

    }

}

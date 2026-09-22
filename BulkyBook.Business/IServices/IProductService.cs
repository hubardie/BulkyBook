using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface IProductService
    { 
        Task<Product?> GetProductByIdASync(int id, bool includeCategory = false);
        Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategory = false);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> IsProductTitleUniqueAsync(string name, int? ProductId = null);
    }

}

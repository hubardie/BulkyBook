using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface IProductService
    { 
        Task<Product?> GetProductByIdASync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> IsProductTitleUniqueAsync(string name, int? ProductId = null);
    }

}


using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface IOrderService
    {
        Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader);
        Task<OrderHeader> GetOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false);

        Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false); 

    }
}

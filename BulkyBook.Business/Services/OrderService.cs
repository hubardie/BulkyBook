using BulkyBook.Business.IServices;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Add(orderHeader);
            await _context.SaveChangesAsync();
            return orderHeader;
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();//  FirstOrDefault(o => o.Id = id);
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product); // ThenInclude: because Product is a property within OrderDetails
            }
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(u => u.ApplicationUserId == userId);
            }
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(u => u.OrderStatus.ToLower() == status.ToLower());
            }
            return await query.ToListAsync();
        }

        public async Task<OrderHeader> GetOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();//  FirstOrDefault(o => o.Id = id);
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product); // ThenInclude: because Product is a property within OrderDetails
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task UpdateOrderAsync(OrderHeader ordeHeader)
        {
            _context.OrderHeaders.Update(ordeHeader);
            await _context.SaveChangesAsync();

        }

    }
}

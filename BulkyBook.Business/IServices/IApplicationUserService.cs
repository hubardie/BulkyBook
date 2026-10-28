using BulkyBook.Models;

namespace BulkyBook.Business.IServices
{
    public interface IApplicationUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);

        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();

    }
}

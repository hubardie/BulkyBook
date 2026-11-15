using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly ApplicationDbContext _dbContext;
        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }
        public async Task InitializeAsync()
        {
            try
            {
                if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _dbContext.Database.MigrateAsync();
                }
                if (!await _roleManager.RoleExistsAsync(SD.RoleAdmin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee));
                    await _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomer));
                }

                ApplicationUser user = await _userManager.FindByEmailAsync("admin@admin.com");
                if (user == null)
                {
                    var result = await _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "admin@admin.com",
                        Email = "admin@admin.com",
                        EmailConfirmed = true,
                        Name = "Admin User",
                        PhoneNumber = "1234567890",
                        StreetAddress = "test",
                        State = "Madrid",
                        PostalCode = "30000",
                        City = "Madrid"


                    }, "Admin123*");

                    if (result.Succeeded)
                    {
                        user = await _userManager.FindByEmailAsync("admin@admin.com");
                        await _userManager.AddToRoleAsync(user, SD.RoleAdmin);
                    }

                }
           
            }

            catch (Exception ex)
            {
                throw;
            }
        
        }
    }
}

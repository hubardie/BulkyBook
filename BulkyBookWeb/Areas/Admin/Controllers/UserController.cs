using BulkyBook.Business.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin + "," +  SD.RoleEmployee)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApplicationUserService _userService;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> RoleManagement(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            RoleManagementVM roleVM = new()
            {
                ApplicationUser = user,
                RoleList = _roleManager.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                })
            };
            roleVM.ApplicationUser.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return View(roleVM);
        }
        [HttpPost]
        public async Task<IActionResult> RoleManagement(RoleManagementVM roleManagementVM)
        {
            var user = await _userService.GetUserByIdAsync(roleManagementVM.ApplicationUser.Id );
            if (user == null)
            {
                return NotFound();
            }
            string oldRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (roleManagementVM.ApplicationUser.Role != oldRole)
            { 
                // update Role
                await _userManager.RemoveFromRoleAsync(user, oldRole);
                await _userManager.AddToRoleAsync(user, roleManagementVM.ApplicationUser.Role);
            }

            TempData["success"] = "Role has been updated";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ChangePassword(string userId)
        {            
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            AdminChangePasswordVM changePasswordVM = new()
            {
                UserEmail = user.Email,                
                UserId = user.Id,

            };
      
            return View(changePasswordVM);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordVM changePasswordVM)
        {
            if(!ModelState.IsValid)
            {
                return View(changePasswordVM);
            }

            var user = await _userService.GetUserByIdAsync(changePasswordVM.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, changePasswordVM.NewPassword);
            if (result.Succeeded)
            {
                TempData["success"] = "Password has been changed";
                return RedirectToAction(nameof(Index));
            }
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            changePasswordVM.UserEmail = user.Email;
            return View(changePasswordVM);
        }

        #region "API CALLS"
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            foreach (var user in users)
            {
                user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            }
            return Json(new { data = users });
        }
        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                return Json(new { success = true, message = "USer locked successfully" });
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1000));
                return Json(new { success = true, message = "User unlocked succesfully" });
            }
        }

        #endregion
    }


}

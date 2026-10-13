using BulkyBook.Business.IServices;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }     
    }
}

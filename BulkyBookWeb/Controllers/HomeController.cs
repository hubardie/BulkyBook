using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }     
    }
}

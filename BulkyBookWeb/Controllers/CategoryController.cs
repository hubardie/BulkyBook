using AspNetCoreGeneratedDocument;
using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;            
        }
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Create")]
        public IActionResult CreatePOST(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) && _context.Categories.Any(c => c.Name.ToLower() == category.Name.ToLower()))
            {
                ModelState.AddModelError("", "Category name already exists");
            }
            if (ModelState.IsValid)
            {             
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Update(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();            
            }

            var category = _context.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            { 
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Update")]
        public IActionResult UpdatePOST(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) && _context.Categories.Any(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id))
            {
                ModelState.AddModelError("", "Category name already exists");
            }
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Delete(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }

            var category = _context.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Delete")]
        public IActionResult DeletePOST(int? Id)
        {
            var category = _context.Categories.Find(Id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

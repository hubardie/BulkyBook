using BulkyBook.Business.IServices;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService service)
        {
            _categoryService = service;            
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllcategoriesAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST(Category category)
        {
            if (!await _categoryService.IsCategoryNameUniqueAsync(category.Name))
            {
                ModelState.AddModelError("", "Category name already exists");
            }
            if (ModelState.IsValid)
            {             
                await _categoryService.CreateCategoryAsync(category);
                TempData["success"] = "Category created succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();            
            }

            var category = await _categoryService.GetCategoryByIdASync(id.Value);

            if (category == null)
            { 
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePOST(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) && !await _categoryService.IsCategoryNameUniqueAsync(category.Name, category.Id))
            {
                ModelState.AddModelError("", "Category name already exists");
            }
            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                TempData["success"] = "Category updated succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdASync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? Id)
        {
            await _categoryService.DeleteCategoryAsync(Id.Value);
            TempData["success"] = "Category deleted succesfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

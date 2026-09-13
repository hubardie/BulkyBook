using BulkyBook.Business.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService service, ICategoryService categoryService)
        {
            _productService = service;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Upsert()
        {
            var categories = await _categoryService.GetAllcategoriesAsync();

            ProductVM productVm = new()
            {
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };

            return View(productVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(Product product, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(product);
                TempData["success"] = "Product created succesfully";
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

            var product = await _productService.GetProductByIdASync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? Id)
        {
            await _productService.DeleteProductAsync(Id.Value);
            TempData["success"] = "Product deleted succesfully";
            return RedirectToAction(nameof(Index));
        }

        #region "API CALLS"
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync(true);
            return Json(new { data = products });
        }
        #endregion
    }


}

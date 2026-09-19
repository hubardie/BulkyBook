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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductService service, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = service;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Upsert(int? id = null)
        {
            var categories = await _categoryService.GetAllcategoriesAsync();

            ProductVM productVm = new()
            {
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };
            if (id == null || id == 0)
            {
                //Create
                productVm.Product = new Product();
            }
            else
            {
                // update
                productVm.Product = await _productService.GetProductByIdASync(id.Value);
            }
            return View(productVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Upsert")]
        public async Task<IActionResult> UpsertPOST(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {

                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine("images", "products");
                    string finalPath = Path.Combine(wwwRootPath, productPath);


                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    // save the new image
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = Path.Combine(@"\", productPath, fileName).Replace("\\", "/");

                }
                if (productVM.Product.Id == null)
                {
                    await _productService.CreateProductAsync(productVM.Product);
                }
                else
                {
                    await _productService.UpdateProductAsync(productVM.Product);
                }
                TempData["success"] = "Product created succesfully";
                return RedirectToAction("Index");
            }
            else
            {
                var categories = await _categoryService.GetAllcategoriesAsync();
                productVM = new()
                {
                    CategoryList = categories.Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }),
                    Product = new Product()
                };
                return View(productVM);
            }
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

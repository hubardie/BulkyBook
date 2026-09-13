using BulkyBook.Business.IServices;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService service)
        {
            _productService = service;            
        }
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST(Product product)
        {
            if (ModelState.IsValid)
            {             
                await _productService.CreateProductAsync(product);
                TempData["success"] = "Product created succesfully";
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

            var product = await _productService.GetProductByIdASync(id.Value);

            if (product == null)
            { 
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Validates that the request comes from the webpage
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePOST(Product product)
        {          
            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(product);
                TempData["success"] = "Product  updated succesfully";
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
    }
}

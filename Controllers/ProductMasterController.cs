using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace localshopyNew.Controllers
{
    public class ProductMasterController : Controller
    {
        private readonly IProductMasterService _service;
        private readonly ICategoryService _categoryService;

        public ProductMasterController(IProductMasterService service, ICategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _service.GetActiveProducts();
            return View(locations);
        }

        public async Task<IActionResult> Create()
        {
            var categoryList = await _categoryService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                return RedirectToAction("Index", "Category");
            }

            ViewBag.CategoryList = new SelectList(categoryList, "Id", "Name"); ;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductMaster location)
        {
            if (string.IsNullOrEmpty(location.ProductName))
                return View(location);

            bool isNameExists = await _service.IsProductNameExists(location.ProductName);
            if (isNameExists)
            {
                ViewBag.ErrorMessage = "Product Name already exists";
                return View(location);
            }
            bool isAdded = await _service.AddProduct(location);
            if (isAdded)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ErrorMessage = "Product Not Added";
            return View(location);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var location = await _service.GetProductById(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductMaster model)
        {
            if (ModelState.IsValid)
            {
                var existsingProduct = await _service.GetProductById(model.Id);
                if (existsingProduct == null) return NotFound();

                if (existsingProduct.ProductName != model.ProductName)
                {

                    bool isNameExists = await _service.IsProductNameExists(model.ProductName);
                    if (isNameExists)
                    {
                        ViewBag.ErrorMessage = "Product Name already exists";
                        return View(model);
                    }
                }
                await _service.UpdateProduct(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var location = await _service.GetProductById(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var location = await _service.GetProductById(id);
            if (location != null)
            {
                location.IsActive = false;
                bool isUpdated = await _service.UpdateProduct(location);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deleted()
        {
            var locations = await _service.GetInActiveProducts();
            return View(locations);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var location = await _service.GetProductById(id);
            if (location != null)
            {
                location.IsActive = true;
                bool isUpdated = await _service.UpdateProduct(location);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Deleted));
                }
            }
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost, ActionName("RemoveFromDatabase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromDatabase(Guid id)
        {
            bool isDeleted = await _service.DeleteProduct(id);
            if (isDeleted)
            {
                return RedirectToAction(nameof(Deleted));
            }
            ViewBag.ErrorMessage = "Product Not Deleted";
            return RedirectToAction(nameof(Deleted));
        }
    }
}

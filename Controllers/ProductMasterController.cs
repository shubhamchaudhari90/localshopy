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
            var products = await _service.GetActiveProducts();
            return View(products);
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
        public async Task<IActionResult> Create(ProductMaster product)
        {
            if (string.IsNullOrEmpty(product.ProductName))
                return View(product);

            bool isNameExists = await _service.IsProductNameExists(product.ProductName);
            if (isNameExists)
            {
                ViewBag.ErrorMessage = "Product Name already exists";
                return View(product);
            }
            bool isAdded = await _service.AddProduct(product);
            if (isAdded)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ErrorMessage = "Product Not Added";
            return View(product);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _service.GetProductById(id);
            if (product == null) return RedirectToAction(nameof(Index));
            Categoty? category = await _categoryService.GetCategoryById(product.CategoryId);
            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryName = category.Name;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductMaster model)
        {
            if (ModelState.IsValid)
            {
                var existsingProduct = await _service.GetProductById(model.Id);
                if (existsingProduct == null) return RedirectToAction(nameof(Index));

                if (existsingProduct.ProductName != model.ProductName)
                {

                    bool isNameExists = await _service.IsProductNameExists(model.ProductName);
                    if (isNameExists)
                    {
                        Categoty? category = await _categoryService.GetCategoryById(model.CategoryId);
                        if (category == null)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        ViewBag.CategoryName = category.Name;
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
            var product = await _service.GetProductById(id);
            if (product == null) return RedirectToAction(nameof(Index));
            Categoty? category = await _categoryService.GetCategoryById(product.CategoryId);
            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryName = category.Name;
            if (product == null) return RedirectToAction(nameof(Index));
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _service.GetProductById(id);
            if (product != null)
            {
                product.IsActive = false;
                bool isUpdated = await _service.UpdateProduct(product);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deleted()
        {
            var products = await _service.GetInActiveProducts();
            return View(products);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var product = await _service.GetProductById(id);
            if (product != null)
            {
                product.IsActive = true;
                bool isUpdated = await _service.UpdateProduct(product);
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

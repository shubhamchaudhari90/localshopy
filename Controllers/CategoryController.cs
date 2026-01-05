using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]

    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _service.GetActiveCategories();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoty category)
        {
            if (string.IsNullOrEmpty(category.Name))
                return View(category);

            bool isNameExists = await _service.IsCategoryNameExists(category.Name);
            if (isNameExists)
            {
                ViewData["ErrorMessage"] = "Category Name already exists";
                return View(category);
            }
            bool isAdded = await _service.AddCategory(category);
            if (isAdded)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewData["ErrorMessage"] = "Category Not Added";
            return View(category);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _service.GetCategoryById(id);
            if (category == null) return RedirectToAction(nameof(Index));
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Categoty model)
        {
            if (ModelState.IsValid)
            {
                var existsingCategory = await _service.GetCategoryById(model.Id);
                if (existsingCategory == null) return RedirectToAction(nameof(Index));
                if (existsingCategory.Name != model.Name)
                {
                    bool isNameExists = await _service.IsCategoryNameExists(model.Name);
                    if (isNameExists)
                    {
                        ViewData["ErrorMessage"] = "Category Name already exists";
                        return View(model);
                    }
                }
                await _service.UpdateCategory(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _service.GetCategoryById(id);
            if (category == null) return RedirectToAction(nameof(Index));
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _service.GetCategoryById(id);
            if (category != null)
            {
                category.IsActive = false;
                bool isUpdated = await _service.UpdateCategory(category);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deleted()
        {
            var categories = await _service.GetInActiveCategories();
            return View(categories);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var category = await _service.GetCategoryById(id);
            if (category != null)
            {
                category.IsActive = true;
                bool isUpdated = await _service.UpdateCategory(category);
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
            bool isDeleted = await _service.DeleteCategory(id);
            if (isDeleted)
            {
                return RedirectToAction(nameof(Deleted));
            }
            ViewData["ErrorMessage"] = "Category Not Deleted";
            return RedirectToAction(nameof(Deleted));
        }
    }
}

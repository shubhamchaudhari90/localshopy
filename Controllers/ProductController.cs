using localshopyNew.Models;
using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ProductController : Controller
    {
        private readonly CategoryService _service;

        public ProductController(CategoryService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View(_service.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductMaster category, string products)
        {
            category.ProductName = products
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase) // makes items unique (case-insensitive)
                .ToList();

            _service.Add(category);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var category = _service.GetById(id);
            if (category == null) return NotFound();

            ViewBag.Products = string.Join(", ", category.ProductName);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(ProductMaster category, string products)
        {
            category.ProductName = products
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase) // makes items unique (case-insensitive)
                .ToList();

            _service.Update(category);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

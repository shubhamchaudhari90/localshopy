using localshopyNew.Models;
using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopService _service;

        public ShopController(ShopService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Shop shop)
        {
            if (!ModelState.IsValid)
                return View(shop);

            try
            {
                string shopId = Guid.NewGuid().ToString();
                shop.Id = shopId;
                _service.CreateShop(shop);
                return RedirectToAction("Details", new { name = shop.Name });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(shop);
            }
        }

        public IActionResult Details(string name)
        {
            try
            {
                var shop = _service.GetShop(name);
                return View(shop);
            }
            catch
            {
                return NotFound();
            }
        }

        public IActionResult Index()
        {
            var shops = _service.GetAllShops();
            return View(shops);
        }

        [HttpGet]
        public IActionResult Edit(string name)
        {
            try
            {
                var shop = _service.GetShop(name);
                return View(shop);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Edit(Shop shop)
        {
            if (!ModelState.IsValid)
                return View(shop);

            try
            {
                _service.UpdateShop(shop);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(shop);
            }
        }

        [HttpPost]
        public IActionResult ToggleOpenStatus(string name)
        {
            try
            {
                var shop = _service.GetShop(name);
                shop.IsOpen = !shop.IsOpen;
                _service.UpdateShop(shop);
                return RedirectToAction("Details", new { id = name });
            }
            catch
            {
                return NotFound();
            }
        }

    }
}

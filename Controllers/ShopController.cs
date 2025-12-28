using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopService _service;
        private readonly LocationService _locationService;

        public ShopController(ShopService service, LocationService locationService)
        {
            _service = service;
            _locationService = locationService;
        }

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    ViewBag.Locations = _locationService.GetAllLocations();
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Create(Shop shop)
        //{
        //    ViewBag.Locations = _locationService.GetAllLocations();
        //    if (!ModelState.IsValid)
        //        return View(shop);
        //    try
        //    {
        //        string shopId = Guid.NewGuid().ToString();
        //        shop.Id = shopId;
        //        _service.CreateShop(shop);
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return View(shop);
        //    }
        //}

        //public IActionResult Details(string name)
        //{
        //    try
        //    {
        //        var shop = _service.GetShop(name);
        //        return View(shop);
        //    }
        //    catch
        //    {
        //        return NotFound();
        //    }
        //}

        //public IActionResult Index()
        //{
        //    var shops = _service.GetAllShops();
        //    return View(shops);
        //}

        //[HttpGet]
        //public IActionResult Edit(string name)
        //{
        //    try
        //    {
        //        var shop = _service.GetShop(name);
        //        ViewBag.Locations = _locationService.GetAllLocations();
        //        return View(shop);
        //    }
        //    catch
        //    {
        //        return NotFound();
        //    }
        //}

        //[HttpPost]
        //public IActionResult Edit(Shop shop)
        //{
        //    if (!ModelState.IsValid)
        //        return View(shop);
        //    try
        //    {
        //        _service.UpdateShop(shop);
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return View(shop);
        //    }
        //}

        //[HttpPost]
        //public IActionResult ToggleOpenStatus(string name)
        //{
        //    try
        //    {
        //        var shop = _service.GetShop(name);
        //        shop.IsOpen = !shop.IsOpen;
        //        _service.UpdateShop(shop);
        //        return RedirectToAction("Details", new { id = name });
        //    }
        //    catch
        //    {
        //        return NotFound();
        //    }
        //}
    }
}

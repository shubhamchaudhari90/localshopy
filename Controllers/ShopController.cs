using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace localshopyNew.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ILocationService _locationService;

        public ShopController(IShopService shopService, ILocationService locationService)
        {
            _locationService = locationService;
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            var shops = await _shopService.GetInActiveShops();
            return View(shops);
        }

        public async Task<IActionResult> Create()
        {
            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null || locationList.Count <= 0)
            {
                return RedirectToAction("Index", "Location");
            }
            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shop shop)
        {
            var locationList = await _locationService.GetActiveLocations();
            if (string.IsNullOrEmpty(shop.Name))
            {
                if (locationList == null || locationList.Count <= 0)
                {
                    return RedirectToAction("Index", "Location");
                }
                ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
                return View(shop);
            }

            bool isNameExists = await _shopService.IsShopNameExists(shop.Name);
            if (isNameExists)
            {
                ViewBag.ErrorMessage = "Shop Name already exists";

                if (locationList == null || locationList.Count <= 0)
                {
                    return RedirectToAction("Index", "Location");
                }
                ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
                return View(shop);
            }
            bool isAdded = await _shopService.AddShop(shop);
            if (isAdded)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ErrorMessage = "Shop Not Added";
            if (locationList == null || locationList.Count <= 0)
            {
                return RedirectToAction("Index", "Location");
            }
            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
            return View(shop);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            Shop? shop = await _shopService.GetShopById(id);

            if (shop == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null || locationList.Count <= 0)
            {
                return RedirectToAction("Index", "Location");
            }
            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");

            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Shop shop)
        {
            var locationList = await _locationService.GetActiveLocations();
            if (string.IsNullOrEmpty(shop.Name))
            {
                if (locationList == null || locationList.Count <= 0)
                {
                    return RedirectToAction("Index", "Location");
                }
                ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
                return View(shop);
            }

            bool isNameExists = await _shopService.IsShopNameExists(shop.Name);
            if (isNameExists)
            {
                ViewBag.ErrorMessage = "Shop Name already exists";

                if (locationList == null || locationList.Count <= 0)
                {
                    return RedirectToAction("Index", "Location");
                }
                ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
                return View(shop);
            }
            bool isAdded = await _shopService.UpdateShop(shop);
            if (isAdded)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ErrorMessage = "Shop Not Added";
            if (locationList == null || locationList.Count <= 0)
            {
                return RedirectToAction("Index", "Location");
            }
            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
            return View(shop);
        }


    }
}

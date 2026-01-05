using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = "Admin")]

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
            var shops = await _shopService.GetActiveShops();
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

            if (!Regex.IsMatch(shop.Name, "^[a-zA-Z0-9 -]+$"))
            {
                ViewData["ErrorMessage"] = "Shop name contains a to z, A to Z and - (hyphen) only";

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
                ViewData["ErrorMessage"] = "Shop Name already exists";

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
            ViewData["ErrorMessage"] = "Shop Not Added";
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
            if (locationList == null || locationList.Count <= 0)
                return RedirectToAction("Index", "Location");

            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
            if (string.IsNullOrEmpty(shop.Name))
                return View(shop);

            bool isUpdated = await _shopService.UpdateShop(shop);
            if (isUpdated)
                return RedirectToAction(nameof(Index));
            return View(shop);
        }

        public async Task<IActionResult> Delete(Guid id)
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

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null || locationList.Count <= 0)
            {
                return RedirectToAction("Index", "Location");
            }
            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");
            var shop = await _shopService.GetShopById(id);
            if (shop == null) return View(shop);
            shop.IsActive = false;
            bool isUpdated = await _shopService.UpdateShop(shop);
            if (isUpdated)
                return RedirectToAction(nameof(Index));
            return View(shop);
        }
        public async Task<IActionResult> Deleted()
        {
            var products = await _shopService.GetInActiveShops();
            return View(products);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var shop = await _shopService.GetShopById(id);
            if (shop != null)
            {
                shop.IsActive = true;
                bool isUpdated = await _shopService.UpdateShop(shop);
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
            bool isDeleted = await _shopService.DeleteShop(id);
            if (isDeleted)
            {
                return RedirectToAction(nameof(Deleted));
            }
            ViewData["ErrorMessage"] = "Shop Not Deleted";
            return RedirectToAction(nameof(Deleted));
        }
    }
}

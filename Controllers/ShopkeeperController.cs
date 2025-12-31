using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace localshopyNew.Controllers
{
    public class ShopkeeperController : Controller
    {
        private readonly IShopkeeperService _shopkeeperService;
        private readonly IEncodingService _encodingService;
        private readonly ILocationService _locationService;

        public ShopkeeperController(IShopkeeperService shopkeeperService, IEncodingService encodingService, ILocationService locationService)
        {
            _encodingService = encodingService;
            _shopkeeperService = shopkeeperService;
            _locationService = locationService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return View();
            }

            Shop? shop = await _shopkeeperService.GetShopByLoginModel(model);
            if (shop == null)
            {
                ViewData["ErrorMessage"] = "Email Id OR Password not match";
                return View();
            }
            string shopIdKey = _encodingService.Encode("ShopId");
            string shopIdValue = _encodingService.Encode(shop.Id.ToString());

            HttpContext.Session.SetString(shopIdKey, shopIdValue);
            return RedirectToAction(nameof(ShopDetails));
        }

        public async Task<IActionResult> ShopDetails()
        {
            Guid shopId = GetShopIdFromSession();
            ShopProductsViewModel model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null)
                RedirectToAction(nameof(Login));
            return View(model);
        }

        public IActionResult Logout()
        {


            // Remove a specific key
            HttpContext.Session.Remove("UserName");

            // Or remove all session data
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Edit()
        {
            Guid shopId = GetShopIdFromSession();
            ShopProductsViewModel model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null || model.Shop == null)
                RedirectToAction(nameof(Login));

            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null)
            {
                ViewData["ErrorMessage"] = "Locations are not active";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");

            return View(model.Shop);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shop shop)
        {
            Guid shopId = GetShopIdFromSession();
            shop.Id = shopId;
            ShopProductsViewModel model = await _shopkeeperService.UpdateShopData(shop);
            if (model == null || model.Shop == null)
                RedirectToAction(nameof(Login));
            return RedirectToAction(nameof(ShopDetails), model);
        }

        private Guid GetShopIdFromSession()
        {
            string shopIdKey = _encodingService.Encode("ShopId");
            string? encodedShopId = HttpContext.Session.GetString(shopIdKey);
            if (string.IsNullOrEmpty(encodedShopId))
            {
                RedirectToAction(nameof(Login));
            }
            string shopIdValue = _encodingService.Decode(encodedShopId);
            Guid shopId = Guid.Parse(shopIdValue);
            return shopId;
        }
    }
}

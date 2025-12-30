using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ShopkeeperController : Controller
    {
        private readonly IShopkeeperService _shopkeeperService;
        private readonly IEncodingService _encodingService;

        public ShopkeeperController(IShopkeeperService shopkeeperService, IEncodingService encodingService)
        {
            _encodingService = encodingService;
            _shopkeeperService = shopkeeperService;
        }

        public IActionResult Index()
        {
            var x = _encodingService.Encode("UserName");
            var x1 = _encodingService.Encode(Guid.NewGuid().ToString());
            var x2 = _encodingService.Encode(x);
            var x3 = _encodingService.Encode(x2);

            var y = _encodingService.Decode(x);
            var y1 = _encodingService.Decode(x1);
            var y2 = _encodingService.Decode(x2);
            var y3 = _encodingService.Decode(x3);


            return View();
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
            string shopIdKey = _encodingService.Encode("ShopId");
            string? encodedShopId = HttpContext.Session.GetString(shopIdKey);
            if (string.IsNullOrEmpty(encodedShopId))
            {
                RedirectToAction(nameof(Login));
            }
            string shopIdValue = _encodingService.Decode(encodedShopId);
            Guid shopId = Guid.Parse(shopIdValue);
            ShopProductsViewModel model = await _shopkeeperService.GetShopDetailsById(shopId);
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

    }
}

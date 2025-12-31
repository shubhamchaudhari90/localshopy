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
        private readonly ICategoryService _categoryService;

        public ShopkeeperController(IShopkeeperService shopkeeperService, IEncodingService encodingService, ILocationService locationService, ICategoryService categoryService)
        {
            _encodingService = encodingService;
            _shopkeeperService = shopkeeperService;
            _locationService = locationService;
            _categoryService = categoryService;
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
            ShopProductsViewModel? model = await _shopkeeperService.GetShopDetailsById(shopId);
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
            ShopProductsViewModel? model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null || model.Shop == null)
                RedirectToAction(nameof(Login));

            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null)
            {
                ViewData["ErrorMessage"] = "Locations are not active";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");

            if (model?.Shop == null)
            {
                return RedirectToAction(nameof(ShopDetails));
            }

            return View(model.Shop);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shop shop)
        {
            if (string.IsNullOrEmpty(shop.Password))
            {
                ViewData["ErrorMessage"] = "Password is not empty";
                return View(shop);
            }

            Guid shopId = GetShopIdFromSession();
            shop.Id = shopId;

            ShopProductsViewModel? model = await _shopkeeperService.UpdateShopData(shop);
            if (model == null || model.Shop == null)
                RedirectToAction(nameof(Login));
            return RedirectToAction(nameof(ShopDetails), model);
        }

        public async Task<IActionResult> AddProduct()
        {
            var categoryList = await _categoryService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                return RedirectToAction("Index", "Category");
            }

            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");
            Product product = new Product()
            {
                Price = 100,
                DiscountValidFrom = DateTime.Today,
                DiscountValidTill = DateTime.Today
            };
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (Guid.Empty == product.ProductMasterId ||
                string.IsNullOrEmpty(product.Description) ||
                product.Price <= 0 ||
                string.IsNullOrEmpty(product.ImageFileName))
            {
                ViewData["ErrorMessage"] = "Mandatory field missing";
            }
            Guid shopId = GetShopIdFromSession();
            product.ShopId = shopId;
            bool isProductValid = await _shopkeeperService.IsProductValid(product);
            if (isProductValid)
            {
                bool isAdded = await _shopkeeperService.AddProductInShop(product);
                if (isAdded)
                {
                    return RedirectToAction(nameof(ShopDetails));
                }
            }

            var categoryList = await _categoryService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                return RedirectToAction("Index", "Category");
            }
            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");
            ViewData["ErrorMessage"] = "Product already exist or any requied field is missing";
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                return BadRequest();

            var products = await _categoryService.GetProductsByCategoryId(categoryId);

            var result = products.Select(p => new
            {
                id = p.Id,
                productName = p.ProductName
            });

            return Json(result);
        }


        private Guid GetShopIdFromSession()
        {
            string shopIdKey = _encodingService.Encode("ShopId");
            string? encodedShopId = HttpContext.Session.GetString(shopIdKey);
            if (string.IsNullOrEmpty(encodedShopId))
            {
                RedirectToAction(nameof(Login));
            }
            string shopIdValue = _encodingService.Decode(encodedShopId ?? string.Empty);
            Guid shopId = Guid.Parse(shopIdValue);
            return shopId;
        }
    }
}

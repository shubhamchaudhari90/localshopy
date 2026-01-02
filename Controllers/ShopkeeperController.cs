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
        private readonly IWebHostEnvironment _env;

        public ShopkeeperController(IShopkeeperService shopkeeperService, IEncodingService encodingService, ILocationService locationService, ICategoryService categoryService, IWebHostEnvironment env)
        {
            _encodingService = encodingService;
            _shopkeeperService = shopkeeperService;
            _locationService = locationService;
            _categoryService = categoryService;
            _env = env;
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
            if (shopId == Guid.Empty)
            {
                return RedirectToAction(nameof(Login));
            }
            ShopProductsViewModel? model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null)
                return RedirectToAction(nameof(Login));
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
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction(nameof(Login));
            }
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
            var locationList = await _locationService.GetActiveLocations();
            if (locationList == null)
            {
                ViewData["ErrorMessage"] = "Locations are not active";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");


            Guid shopId = GetShopIdFromSession();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction(nameof(Login));
            }
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
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
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
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction(nameof(Login));
            }

            if (product.ProductImage != null && product.ProductImage.Length > 0 && product.ProductImage.Length > 1 * 1024 * 1024)
            {
                ViewData["ErrorMessage"] = "Image must be less than 1 MB";
                return View(product);
            }

            product.ShopId = shopId;

            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploads);

                var extension = Path.GetExtension(product.ProductImage.FileName);
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await product.ProductImage.CopyToAsync(stream);

                product.ImageFileName = fileName;
            }

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
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }
            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");
            ViewData["ErrorMessage"] = "Product already exist or any requied field is missing";
            return View(product);
        }

        public async Task<IActionResult> EditProduct(Guid productId)
        {
            string productIdKey = _encodingService.Encode("ProductId");
            string productIdValue = _encodingService.Encode(productId.ToString());

            HttpContext.Session.SetString(productIdKey, productIdValue);

            var categoryList = await _categoryService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");

            ProductViewModel? product = await _shopkeeperService.GetProductById(productId);
            if (product == null)
            {
                return RedirectToAction(nameof(ShopDetails));
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            var categoryList = await _categoryService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }
            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");

            Guid productId = GetProductIdFromSession();
            if (productId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Product not found";
                return RedirectToAction(nameof(ShopDetails));
            }

            product.Id = productId;

            if (product.Price <= 0)
            {
                ViewData["ErrorMessage"] = "Mandatory field missing";
                return View(product);
            }

            Guid shopId = GetShopIdFromSession();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction(nameof(Login));
            }

            if (product.ProductImage != null && product.ProductImage.Length > 0 && product.ProductImage.Length > 1 * 1024 * 1024)
            {
                ViewData["ErrorMessage"] = "Image must be less than 1 MB";
                return View(product);
            }


            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploads);

                var extension = Path.GetExtension(product.ProductImage.FileName);
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await product.ProductImage.CopyToAsync(stream);

                product.ImageFileName = fileName;
            }

            product.ShopId = shopId;

            string? oldImageName = await _shopkeeperService.UpdateProductInShop(product);
            if (oldImageName != null && product.ProductImage != null && product.ProductImage.Length > 0)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(oldImageName))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "images", "products", oldImageName);

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }
            return RedirectToAction(nameof(ShopDetails));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid productId)
        {
            Guid shopId = GetShopIdFromSession();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction(nameof(Login));
            }
            bool isDeleted = await _shopkeeperService.DeleteProductFromShop(shopId, productId);
            if (!isDeleted)
            {
                ViewData["ErrorMessage"] = "Product Not Deleted.";
            }
            return RedirectToAction(nameof(ShopDetails));
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
                return Guid.Empty;
            }
            string shopIdValue = _encodingService.Decode(encodedShopId ?? string.Empty);
            Guid shopId = Guid.Parse(shopIdValue);
            return shopId;
        }

        private Guid GetProductIdFromSession()
        {
            string productIdKey = _encodingService.Encode("ProductId");
            string? encodedProductId = HttpContext.Session.GetString(productIdKey);
            if (string.IsNullOrEmpty(encodedProductId))
            {
                return Guid.Empty;
            }
            string productIdValue = _encodingService.Decode(encodedProductId ?? string.Empty);
            Guid productId = Guid.Parse(productIdValue);
            return productId;
        }
    }
}

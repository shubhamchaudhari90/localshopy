using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace localshopyNew.Controllers
{

    [Authorize(Roles = RoleConstants.Shopkeeper)]
    public class ShopkeeperController(
        IShopkeeperService shopkeeperService,
        IWebHostEnvironment env,
        IAdminService adminService,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ISessionService sessionService) : Controller
    {
        private readonly IShopkeeperService _shopkeeperService = shopkeeperService;
        private readonly IAdminService _adminService = adminService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly IWebHostEnvironment _env = env;

        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public async Task<IActionResult> ShopDetails()
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Logout", "Account");
            }
            ShopProductsViewModel? model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null)
                return RedirectToAction("Logout", "Account");

            return View(model);
        }

        public async Task<IActionResult> Edit()
        {

            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction("Logout", "Account");
            }
            ShopProductsViewModel? model = await _shopkeeperService.GetShopDetailsById(shopId);
            if (model == null || model.Shop == null)
                return RedirectToAction("Logout", "Account");

            var locationList = await _shopkeeperService.GetActiveLocations();
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
            var locationList = await _shopkeeperService.GetActiveLocations();
            if (locationList == null)
            {
                ViewData["ErrorMessage"] = "Locations are not active";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.LocationList = new SelectList(locationList, "Id", "Name");

            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction("Logout", "Account");
            }
            shop.Id = shopId;

            ShopProductsViewModel? model = await _shopkeeperService.UpdateShopData(shop);
            if (model == null || model.Shop == null)
                return RedirectToAction("Logout", "Account");
            return RedirectToAction(nameof(ShopDetails), model);
        }

        [HttpPost]
        public async Task<bool> SwitchStatus()
        {

            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return false;
            }
            bool isSuccess = await _shopkeeperService.SwitchStatus(shopId);
            if (isSuccess)
                return true;

            TempData["ErrorMessage"] = "Action Required Before Closing the Shop";

            return false;
        }

        public async Task<IActionResult> CategoryList()
        {
            var categoryProductList = await _shopkeeperService.CategoryProductList();
            return View(categoryProductList);
        }

        public async Task<IActionResult> AddProduct()
        {
            var categoryList = await _shopkeeperService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }

            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");
            Product product = new Product()
            {
                Type = ProductTypeConstants.Veg,
                IsAvailable = true,
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
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction("Logout", "Account");
            }

            if (product.ProductImage != null && product.ProductImage.Length > 0 && product.ProductImage.Length > 1 * 1024 * 1024)
            {
                ViewData["ErrorMessage"] = "Image must be less than 1 MB";
                return View(product);
            }

            product.ShopId = shopId;

            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                // Persistent folder in Azure App Service
                var uploadsRoot = Path.Combine("/home/site/wwwroot/uploads/products");
                Directory.CreateDirectory(uploadsRoot);

                var extension = Path.GetExtension(product.ProductImage.FileName);
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploadsRoot, fileName);

                // Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await product.ProductImage.CopyToAsync(stream);

                // Store filename in DB
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

            var categoryList = await _shopkeeperService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }
            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");
            ViewData["ErrorMessage"] = "Product already exist or any requied field is missing";
            await RemoveUnusedImages();
            return View(product);
        }

        public async Task<IActionResult> EditProduct(Guid productId)
        {
            _sessionService.SetProductId(productId);

            var categoryList = await _shopkeeperService.GetActiveCategories();
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
            var categoryList = await _shopkeeperService.GetActiveCategories();
            if (categoryList == null || categoryList.Count <= 0)
            {
                ViewData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(ShopDetails));
            }
            ViewBag.Categories = new SelectList(categoryList, "Id", "Name");

            Guid productId = _sessionService.GetProductId();
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

            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction("Logout", "Account");
            }

            if (product.ProductImage != null && product.ProductImage.Length > 0 && product.ProductImage.Length > 1 * 1024 * 1024)
            {
                ViewData["ErrorMessage"] = "Image must be less than 1 MB";
                return View(product);
            }

            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                // 1. Persistent folder in Azure
                var uploadsRoot = Path.Combine("/home/site/wwwroot/uploads/products");
                Directory.CreateDirectory(uploadsRoot);

                // 2. Generate unique filename
                var extension = Path.GetExtension(product.ProductImage.FileName);
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploadsRoot, fileName);

                // 3. Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await product.ProductImage.CopyToAsync(stream);

                // 4. Store filename in DB
                product.ImageFileName = fileName;
            }


            product.ShopId = shopId;

            string? oldImageName = await _shopkeeperService.UpdateProductInShop(product);
            if (oldImageName != null && product.ProductImage != null && product.ProductImage.Length > 0)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(oldImageName))
                {
                    var oldImagePath = Path.Combine("/home/site/wwwroot/uploads/products", oldImageName);

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
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Session Expired";
                return RedirectToAction("Logout", "Account");
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
            Guid shopId = _sessionService.GetShopId();
            var products = await _shopkeeperService.GetProductsByCategoryId(categoryId, shopId);

            var result = products.Select(p => new
            {
                id = p.Id,
                productName = p.ProductName
            });

            return Json(result);
        }

        private async Task RemoveUnusedImages()
        {
            // Get all image names in DB
            List<string?> imagesInDB = await _shopkeeperService.GetAllImageNames();

            // Path to the persistent product images folder
            string imageFolder = Path.Combine("/home/site/wwwroot/uploads/products");

            if (Directory.Exists(imageFolder))
            {
                // Get all files in folder
                var allFiles = Directory.GetFiles(imageFolder);

                foreach (var filePath in allFiles)
                {
                    string fileName = Path.GetFileName(filePath);

                    // If file is not in DB, delete it
                    if (!imagesInDB.Contains(fileName))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }
    }
}

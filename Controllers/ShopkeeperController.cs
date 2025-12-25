using localshopyNew.Models;
using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ShopkeeperController : Controller
    {
        private readonly ShopkeeperService _service;
        private readonly CategoryService _categoryService;
        private readonly LocationService _locationService;

        public ShopkeeperController(ShopkeeperService service, CategoryService categoryService, LocationService locationService)
        {
            _service = service;
            _categoryService = categoryService;
            _locationService = locationService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            Shop? shop = _service.IsShopExists(email, password);
            if (shop != null && shop.Id != null)
            {
                HttpContext.Session.SetString("ShopLoggedIn", shop.Id);
                return RedirectToAction("Products");
            }
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        // Index of Products
        public IActionResult Products()
        {
            var shop = GetLoggedInShop();
            if (shop == null)
            {
                // If session expired or not logged in, redirect to login
                return RedirectToAction("Login");
            }
            // Pass list of products to the view
            return View(shop.Products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var products = _categoryService.GetAllProducts();

            ViewBag.Products = products;

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product, IFormFile? ProductImage)
        {
            var shop = GetLoggedInShop();
            if (shop == null) return RedirectToAction("Login");

            if (!ModelState.IsValid) return View(product);

            try
            {
                // Handle image upload
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProductImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ProductImage.CopyTo(stream);
                    }

                    product.ImageFileName = fileName;
                }

                _service.AddProduct(shop, product);
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(product);
            }
        }

        [HttpGet]
        public IActionResult EditProduct(string name)
        {
            var shop = GetLoggedInShop();
            if (shop == null)
                return RedirectToAction("Login");

            var product = shop.Products.FirstOrDefault(p => p.Name == name);
            if (product == null)
                return NotFound();

            var products = _categoryService.GetAllProducts();
            ViewBag.Products = products;
            HttpContext.Session.SetString("ProductName", name);
            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product product, IFormFile? ProductImage)
        {
            var shop = GetLoggedInShop();
            if (shop == null) return RedirectToAction("Login");

            if (!ModelState.IsValid) return View(product);

            try
            {
                if (ProductImage != null && ProductImage.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProductImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ProductImage.CopyTo(stream);
                    }

                    product.ImageFileName = fileName;
                }
                string? productName = GetProductNameForEdit();
                if (productName == null) return NotFound();
                product.Name = productName;
                _service.UpdateProduct(shop, product);
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(product);
            }
        }

        public IActionResult DeleteProduct(string name)
        {
            var shop = GetLoggedInShop();
            if (shop == null) return RedirectToAction("Login");

            try
            {
                _service.DeleteProduct(shop, name);
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Products");
            }

        }

        [HttpGet]
        public IActionResult Edit()
        {
            try
            {
                var shopId = HttpContext.Session.GetString("ShopLoggedIn");
                if (shopId == null)
                    return NotFound();
                var shop = _service.GetShopById(shopId);
                if (shop == null)
                    return NotFound();
                ViewBag.Locations = _locationService.GetAllLocations();
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
                var shopId = HttpContext.Session.GetString("ShopLoggedIn");
                if (shopId == null)
                    return NotFound();
                var existing = _service.GetShopById(shopId);

                if (existing != null
                    && existing.Id == shopId
                    && existing.Name == shop.Name
                    && existing.OwnerEmailId == shop.OwnerEmailId
                    && existing.PhoneNo == shop.PhoneNo
                    )
                {
                    _service.UpdateFromShopkeeper(shop);
                    return RedirectToAction("Products");
                }
                ViewBag.Locations = _locationService.GetAllLocations();
                return View(shop);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(shop);
            }
        }

        // Helper: Get logged-in shop from session
        private Shop? GetLoggedInShop()
        {
            var shopId = HttpContext.Session.GetString("ShopLoggedIn");
            if (string.IsNullOrEmpty(shopId))
                return null;
            return _service.GetShopById(shopId);
        }

        private string? GetProductNameForEdit()
        {
            var productName = HttpContext.Session.GetString("ProductName");
            return productName;
        }
    }
}

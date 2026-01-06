using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{
    public class CustomerController(ILocationService locationService, IEncodingService encodingService, ICustomerService customerService) : Controller
    {
        private readonly ILocationService _locationService = locationService;
        private readonly IEncodingService _encodingService = encodingService;
        private readonly ICustomerService _customerService = customerService;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Location()
        {
            List<Location> locations = await _locationService.GetActiveLocations();
            LocationViewModel model = new()
            {
                Locations = locations,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Location(LocationViewModel model)
        {

            List<Location> locations = await _locationService.GetActiveLocations();
            if (locations.Any(x => x.Id == model.SelectedLocationId))
            {
                string locationKey = _encodingService.Encode("Location");
                string locationValue = _encodingService.Encode(model.SelectedLocationId.ToString() ?? "");
                HttpContext.Session.SetString(locationKey, locationValue);

                return RedirectToAction(nameof(Products));
            }

            return View(locations);
        }

        public async Task<IActionResult> ProductsByCategories(string categories)
        {

            var products = await _customerService.GetProductsByCategories(categories);

            return PartialView("_ProductListPartial", products);
        }

        public async Task<IActionResult> Products()
        {
            Guid location = GetLocationFromSession();
            if (location == Guid.Empty)
            {
                return RedirectToAction(nameof(Location));
            }

            var model = await GetCategotiesByLocation();

            return View(model);
        }

        public async Task<List<Category>> GetCategotiesByLocation()
        {
            List<Category> categoties = [];
            Guid location = GetLocationFromSession();
            if (location == Guid.Empty)
            {
                return categoties;
            }

            categoties = await _customerService.GetCategoriesByLocation(location);
            return categoties;
        }

        public async Task<IActionResult> ShopDetails(string shopName)
        {
            string decodedName = shopName.Replace("--", "\u0000").Replace("-", " ").Replace("\u0000", "-");

            var shopDetails = await _customerService.GetShopDetailsByName(decodedName);
            if (shopDetails == null || shopDetails.Shop == null)
            {
                return RedirectToAction(nameof(Location));
            }
            return View(shopDetails);
        }

        public async Task<IActionResult> ProductDetail(string shopProductName)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email)) { email = string.Empty; }

            if (string.IsNullOrEmpty(shopProductName) || !shopProductName.Contains('_'))
            {
                return RedirectToAction("NotFound404", "Error");
            }

            string[] names = shopProductName.Split("_");

            if (names.Length <= 1) { return RedirectToAction("NotFound404", "Error"); }

            string decodedShopName = names[0].Replace("--", "\u0000").Replace("-", " ").Replace("\u0000", "-");
            string decodedProductName = names[1].Replace("--", "\u0000").Replace("-", " ").Replace("\u0000", "-");

            var productDetails = await _customerService.GetProductDetailsByName(decodedShopName, decodedProductName, email);

            if (productDetails == null)
            {
                return RedirectToAction("NotFound404", "Error");
            }
            TempData["shopProductName"] = shopProductName;
            return View(productDetails);
        }

        public async Task<IActionResult> ChangeLocation()
        {
            List<Location> locations = await _locationService.GetActiveLocations();
            string locationKey = _encodingService.Encode("Location");

            // Remove a specific key
            HttpContext.Session.Remove(locationKey);
            return RedirectToAction(nameof(Location));
        }

        [HttpPost]
        public IActionResult AddReview(ReviewViewModel model)
        {

            var data = TempData["shopProductName"];

            if (!ModelState.IsValid)
            {
                // handle validation errors
                return RedirectToAction("ProductDetail", new { shopProductName = data });
            }

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            string reviewer = User.Identity!.Name!;
            // Save review to database
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ProductId = model.ProductId,
                Rating = model.Rating,
                Comment = model.Comment,
                Reviewer = reviewer,
                CreatedAt = DateTime.Now,
                IsApproved = false
            };

            _customerService.AddReview(review);

            return RedirectToAction("ProductDetail", new { shopProductName = data });
        }

        private Guid GetLocationFromSession()
        {
            string locationKey = _encodingService.Encode("Location");
            string? encodedLocation = HttpContext.Session.GetString(locationKey);
            if (string.IsNullOrEmpty(encodedLocation))
            {
                return Guid.Empty;
            }
            string locationValue = _encodingService.Decode(encodedLocation ?? string.Empty);
            Guid location = Guid.Parse(locationValue);
            return location;
        }
    }
}

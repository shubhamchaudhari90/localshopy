using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{

    public class CustomerController(ILocationService locationService, ICustomerService customerService, ICartService cartService, ISessionService sessionService) : Controller
    {
        private readonly ILocationService _locationService = locationService;
        private readonly ICustomerService _customerService = customerService;
        private readonly ICartService _cartService = cartService;
        private readonly ISessionService _sessionService = sessionService;

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

            if (model.SelectedLocationId == null || model.SelectedLocationId == Guid.Empty)
            {
                return View(locations);
            }

            if (locations.Any(x => x.Id == model.SelectedLocationId))
            {
                if (model.SelectedLocationId != Guid.Empty)
                {
                    _sessionService.SetLocation(model.SelectedLocationId ?? Guid.Empty);

                    string? email = User.FindFirstValue(ClaimTypes.Email);

                    if (!string.IsNullOrEmpty(email))
                    {
                        Guid locationId = Guid.Parse(model.SelectedLocationId.ToString());
                        await SetCartCountInSession(email, locationId);
                    }

                    return RedirectToAction(nameof(Products));
                }
            }

            return View(locations);
        }

        public async Task<IActionResult> ProductsByCategories(string categories)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);


            if (string.IsNullOrEmpty(email)) { email = string.Empty; }

            Guid location = _sessionService.GetLocation();
            if (location == Guid.Empty)
            {
                return RedirectToAction(nameof(Location));
            }

            var products = await _customerService.GetProductsByCategories(categories, email, location);

            return PartialView("_ProductListPartial", products);
        }

        public async Task<IActionResult> Products()
        {
            Guid location = _sessionService.GetLocation();
            if (location == Guid.Empty)
            {
                return RedirectToAction(nameof(Location));
            }

            var model = await GetCategotiesByLocation();

            string? email = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(email))
            {
                await SetCartCountInSession(email, location);
            }

            return View(model);
        }

        public async Task<List<Category>> GetCategotiesByLocation()
        {
            List<Category>? categories = [];

            // Get location from session
            Guid location = _sessionService.GetLocation();
            if (location == Guid.Empty)
            {
                return categories;
            }

            // Fetch categories for this location
            categories = await _customerService.GetCategoriesByLocation(location);

            if (categories == null || categories.Count <= 0)
            {
                return [];
            }

            // Shuffle the list randomly
            categories = [.. categories.OrderBy(c => Guid.NewGuid())];

            return categories;
        }

        public async Task<IActionResult> ShopDetails(string shopName)
        {
            //if (string.IsNullOrEmpty(shopName)) { return RedirectToAction("NotFound404", "Error"); }

            string? email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email)) { email = string.Empty; }

            string decodedName = shopName.Replace("--", "'").Replace("_", " ");

            var shopDetails = await _customerService.GetShopDetailsByName(decodedName, email);
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

            string[] names = shopProductName.Split("~");

            if (names == null || names.Length != 2) { return RedirectToAction("NotFound404", "Error"); }

            string decodedShopName = names[0].Replace("--", "'").Replace("_", " ");
            string decodedProductName = names[1].Replace("--", "'").Replace("_", " ");

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
            _sessionService.RemoveLocation();
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

            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            // Save review to database
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ProductId = model.ProductId,
                Rating = model.Rating,
                Comment = model.Comment,
                Reviewer = reviewer,
                CreatedAt = now,
                IsApproved = false
            };

            _customerService.AddReview(review);

            return RedirectToAction("ProductDetail", new { shopProductName = data });
        }

        private async Task SetCartCountInSession(string emailId, Guid location)
        {
            var cartProducts = await _cartService.GetCartDetails(emailId, location);
            if (cartProducts != null && cartProducts.Count > 0)
                _sessionService.SetCartCount(cartProducts.Sum(x => x.Quantity));
            else
                _sessionService.SetCartCount(0);
        }
    }
}

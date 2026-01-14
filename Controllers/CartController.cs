using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _service;
        private readonly IEncodingService _encodingService;
        private readonly ICartService _cartService;

        public CartController(ICartService service, IEncodingService encodingService, ICartService cartService)
        {
            _service = service;
            _encodingService = encodingService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                email = string.Empty;
            }
            Guid locationId = GetLocationFromSession();
            if (locationId == Guid.Empty)
            {
                return RedirectToAction("Location", "Customer");
            }
            List<CartViewModel> products = await _service.GetCartDetails(email, locationId);
            if (products != null || products?.Count > 0)
            {
                SetCartCountInSession(products.Count);
            }
            return View(products);
        }

        [HttpPost]
        public async Task<bool> AddProductToCart(string productName, string shopName)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                bool result = await _cartService.AddProductToCart(productName, shopName, email);
                return result;
            }
            return false;
        }

        [HttpPost]
        public async Task<bool> RemoveProductFromCart(Guid id)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                bool result = await _cartService.RemoveProductFromCart(id, email);
                return result;
            }
            return false;
        }

        [HttpPost]
        public async Task<bool> UpdateCartQuantity(Guid productId, int quantity)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                bool result = await _cartService.UpdateCartQuantity(productId, quantity, email);
                return result;
            }
            return false;
        }

        [HttpPost]
        public IActionResult RemoveFromCart(Guid productId)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Unauthorized" });

            _cartService.RemoveProductFromCart(productId, email);

            return Json(new { success = true });
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

        private int GetCartCountFromSession()
        {
            string cartCountKey = _encodingService.Encode("CartCount");
            string? encodedCartCount = HttpContext.Session.GetString(cartCountKey);
            if (string.IsNullOrEmpty(encodedCartCount))
            {
                return 0;
            }
            string cartCountValue = _encodingService.Decode(encodedCartCount ?? string.Empty);
            int cartCount = 0;
            if (!int.TryParse(cartCountValue, out cartCount))
            {
                return 0;
            }
            return cartCount;
        }

        private void SetCartCountInSession(int count)
        {
            string cartCountKey = _encodingService.Encode("CartCount");
            string cartCountValue = _encodingService.Encode(count.ToString());
            HttpContext.Session.SetString(cartCountKey, cartCountValue);
        }
    }
}

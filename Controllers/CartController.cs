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

            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = GetLocationFromSession();
                if (locationId == Guid.Empty)
                {
                    return RedirectToAction("Location", "Customer");
                }
                List<CartViewModel> products = await _service.GetCartDetails(email, locationId);
                if (products != null || products?.Count > 0)
                {
                    HttpContext.Session.SetInt32("CartCount", products.Sum(x => x.Quantity));
                }
                return View(products);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<bool> AddProductToCart(string productName, string shopName)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                bool result = await _cartService.AddProductToCart(productName, shopName, email);
                int cartCount = GetCartCountFromSession();
                HttpContext.Session.SetInt32("CartCount", cartCount + 1);
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
                Guid locationId = GetLocationFromSession();
                if (locationId != Guid.Empty)
                    await SetCartCountInSession(email, locationId);
                return result;
            }
            return false;
        }

        [HttpPost]
        public async Task<int> GetUpdatedCartCountSession()
        {
            int cartCount = 0;
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = GetLocationFromSession();
                if (locationId != Guid.Empty)
                    cartCount = await SetCartCountInSession(email, locationId);
            }
            return cartCount;
        }

        [HttpPost]
        public async Task<bool> UpdateCartQuantity(Guid productId, int quantity, bool isIncrease)
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                bool result = await _cartService.UpdateCartQuantity(productId, quantity, email);
                int cartCount = GetCartCountFromSession();
                cartCount = isIncrease ? cartCount - 1 : cartCount + 1;
                HttpContext.Session.SetInt32("CartCount", cartCount);
                return result;
            }
            return false;
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
            int cartCountValue = HttpContext.Session.GetInt32("CartCount") ?? 0;
            return cartCountValue;
        }

        private async Task<int> SetCartCountInSession(string emailId, Guid location)
        {
            int cartCount = 0;
            var cartProducts = await _cartService.GetCartDetails(emailId, location);
            if (cartProducts != null && cartProducts.Count > 0)
            {
                cartCount = cartProducts.Sum(x => x.Quantity);
                HttpContext.Session.SetInt32("CartCount", cartCount);
            }
            return cartCount;
        }
    }
}

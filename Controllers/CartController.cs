using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{
    [Authorize]
    public class CartController(ICartService cartService, ISessionService sessionService) : Controller
    {
        private readonly ICartService _cartService = cartService;
        private readonly ISessionService _sessionService = sessionService;

        public async Task<IActionResult> Index()
        {
            string? email = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = _sessionService.GetLocation();
                if (locationId == Guid.Empty)
                {
                    return RedirectToAction("Location", "Customer");
                }
                List<CartViewModel> products = await _cartService.GetCartDetails(email, locationId);
                if (products != null || products?.Count > 0)
                    _sessionService.SetCartCount(products.Sum(x => x.Quantity));
                else
                    _sessionService.SetCartCount(0);
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
                int cartCount = _sessionService.GetCartCount();
                if (result)
                    _sessionService.SetCartCount(cartCount + 1);
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
                Guid locationId = _sessionService.GetLocation();
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
                Guid locationId = _sessionService.GetLocation();
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

                if (result)
                {
                    int cartCount = _sessionService.GetCartCount();
                    cartCount = isIncrease ? cartCount - 1 : cartCount + 1;
                    _sessionService.SetCartCount(cartCount);
                }
                return result;
            }
            return false;
        }

        private async Task<int> SetCartCountInSession(string emailId, Guid location)
        {
            int cartCount = 0;
            List<CartViewModel> cartProducts = await _cartService.GetCartDetails(emailId, location);
            if (cartProducts != null && cartProducts.Count > 0)
            {
                cartCount = cartProducts.Sum(x => x.Quantity);
                _sessionService.SetCartCount(cartCount);
            }
            else
                _sessionService.SetCartCount(0);
            return cartCount;
        }
    }
}

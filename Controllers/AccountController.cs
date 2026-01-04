using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{
    public class AccountController : Controller
    {
        private readonly IShopkeeperService _shopkeeperService;
        private readonly IEncodingService _encodingService;
        private readonly IAdminService _adminService;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            IShopkeeperService shopkeeperService,
            IEncodingService encodingService,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IAdminService adminService)
        {
            _encodingService = encodingService;
            _shopkeeperService = shopkeeperService;
            _signInManager = signInManager;
            _userManager = userManager;
            _adminService = adminService;
        }

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            // Sign in if external login exists
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                false);

            if (!signInResult.Succeeded)
            {
                // Create Identity user if not exists
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email
                };

                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, false);
            }

            string admin = _adminService.AdminLoggedInFromGoogle(email);

            if (!string.IsNullOrEmpty(admin))
            {
                HttpContext.Session.SetString("admin", admin);
                await SetRole(email, "Admin");
                return RedirectToAction("Index", "Location");
            }

            var shopDetails = await _shopkeeperService.GetShopDetailsByEmailId(email);

            if (shopDetails == null)
            {
                return RedirectToAction("Products", "Customer");
            }
            else
            {
                string shopIdKey = _encodingService.Encode("ShopId");
                string shopIdValue = _encodingService.Encode(shopDetails.Shop.Id.ToString());

                HttpContext.Session.SetString(shopIdKey, shopIdValue);

                if (shopDetails.Shop.OwnerEmailId != "")
                {
                    HttpContext.Session.SetString("IsShopkeeper", "TRUE");
                    await SetRole(email, "Shopkeeper");
                }
                return RedirectToAction("ShopDetails", "Shopkeeper");
            }
        }

        [HttpGet]
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
            string admin = _adminService.AdminLoggedIn(model.Email, model.Password);

            if (!string.IsNullOrEmpty(admin))
            {
                HttpContext.Session.SetString("admin", admin);

                await SetRole(model.Email, "Admin");

                return RedirectToAction("Index", "Location");
            }

            Shop? shop = await _shopkeeperService.GetShopByLoginModel(model);
            if (shop == null || shop.Id == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Email Id OR Password not match";
                return View();
            }
            string shopIdKey = _encodingService.Encode("ShopId");
            string shopIdValue = _encodingService.Encode(shop.Id.ToString());

            HttpContext.Session.SetString(shopIdKey, shopIdValue);

            if (model.Email != "")
            {
                HttpContext.Session.SetString("IsShopkeeper", "TRUE");
                await SetRole(model.Email, "Shopkeeper");
            }
            return RedirectToAction("ShopDetails", "Shopkeeper");
        }

        public async Task<IActionResult> Logout()
        {

            string shopIdKey = _encodingService.Encode("ShopId");

            await _signInManager.SignOutAsync();

            // Remove a specific key
            HttpContext.Session.Remove("shopIdKey");
            HttpContext.Session.Remove("IsShopkeeper");

            // Or remove all session data
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }


        private async Task SetRole(string email, string role)
        {
            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in using ASP.NET Core Identity cookie scheme
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        }
    }
}

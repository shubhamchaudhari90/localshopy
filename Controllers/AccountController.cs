using localshopyNew.Constants;
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
        private readonly IAdminService _adminService;
        private readonly ISessionService _sessionService;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            IShopkeeperService shopkeeperService,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IAdminService adminService, ISessionService sessionService)
        {
            _shopkeeperService = shopkeeperService;
            _signInManager = signInManager;
            _userManager = userManager;
            _adminService = adminService;
            _sessionService = sessionService;
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
                _sessionService.SetString(RoleConstants.Admin, admin);
                await SetRole(email, RoleConstants.Admin);
                return RedirectToAction("Index", "Review");
            }

            var shopDetails = await _shopkeeperService.GetShopDetailsByEmailId(email);

            if (shopDetails == null || shopDetails.Shop == null)
            {
                return RedirectToAction("Products", "Customer");
            }
            else
            {
                _sessionService.SetShopId(shopDetails.Shop.Id);

                if (shopDetails.Shop.OwnerEmailId != "")
                {
                    _sessionService.SetString(RoleConstants.IsShopkeeper, "TRUE");
                    await SetRole(email, RoleConstants.Shopkeeper);
                }
                return RedirectToAction("OrdersToServe", "Shopkeeper");
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
                _sessionService.SetString(RoleConstants.Admin, admin);

                await SetRole(model.Email, RoleConstants.Admin);

                return RedirectToAction("Index", "Review");
            }

            Shop? shop = await _shopkeeperService.GetShopByLoginModel(model);
            if (shop == null || shop.Id == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Email Id OR Password not match";
                return View();
            }

            _sessionService.SetShopId(shop.Id);

            if (model.Email != "")
            {
                _sessionService.SetString(RoleConstants.IsShopkeeper, "TRUE");
                await SetRole(model.Email, RoleConstants.Shopkeeper);
            }
            return RedirectToAction("OrdersToServe", "Order");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _sessionService.Logout();
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

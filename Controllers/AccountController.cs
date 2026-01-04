using localshopyNew.Services.Interfaces;
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
                return RedirectToAction("Index", "Location");
            }

            // 🔍 CHECK EMAIL IN TABLE1
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
                    HttpContext.Session.SetString("IsShopkeeper", "TRUE");

                return RedirectToAction("ShopDetails", "Shopkeeper");
            }
        }
    }
}

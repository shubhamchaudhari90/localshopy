using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstant.Admin)]
    public class ShopProductController : Controller
    {
        private readonly IShopProductService _shopProductService;
        private readonly IShopService _shopService;

        public ShopProductController(IShopProductService shopProductService, IShopService shopService)
        {
            _shopProductService = shopProductService;
            _shopService = shopService;
        }
        public async Task<IActionResult> ShopList()
        {
            List<Shop> shops = await _shopService.GetAllShops();
            return View(shops);
        }

        public async Task<IActionResult> ShopProducts(Guid shopId)
        {
            var shopProducts = await _shopProductService.GetAllShopProduct(shopId);
            return View(shopProducts);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid productId)
        {
            var product = await _shopProductService.GetProductDetails(productId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            await _shopProductService.UpdateProduct(model);
            return RedirectToAction("ShopProducts", new { shopId = model.ShopId });
        }
    }
}

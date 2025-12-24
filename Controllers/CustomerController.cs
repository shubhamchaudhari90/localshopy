using localshopyNew.Models;
using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerService _customerService;
        private readonly ShopService _shopService;

        public CustomerController(CustomerService customerService, ShopService shopService)
        {
            _customerService = customerService;
            _shopService = shopService;
        }

        // GET: /Customer/Products
        public IActionResult Products()
        {
            var selectedLocation = HttpContext.Session.GetString("SelectedLocation");
            var allProducts = _customerService.GetAllProducts(selectedLocation);
            if (selectedLocation == null || !_customerService.IsLocationValid(selectedLocation))
            {
                return Location();
            }
            return View(allProducts);
        }

        public IActionResult Location()
        {
            var locations = _customerService.GetAllLocations(); // List<string>
            return View(locations);
        }

        [HttpPost]
        public IActionResult Location(string location)
        {
            HttpContext.Session.SetString("SelectedLocation", location);
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult ShopDetails(string shopName)
        {
            Shop shop = _shopService.GetShop(shopName);



            return View(shop);
        }
    }
}

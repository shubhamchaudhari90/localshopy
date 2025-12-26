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

        public IActionResult Products()
        {
            var selectedLocation = HttpContext.Session.GetString("SelectedLocation");
            if (string.IsNullOrEmpty(selectedLocation))
            {
                return RedirectToAction("Location");
            }
            ViewBag.SelectedLocation = selectedLocation;
            var allProducts = _customerService.GetAllProducts(selectedLocation);
            if (selectedLocation == null || !_customerService.IsLocationValid(selectedLocation))
            {
                return RedirectToAction("Location");
            }
            ViewBag.CurrentPage = "Products";
            return View(allProducts);
        }

        public IActionResult Location()
        {
            ViewBag.CurrentPage = "Location";
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
            List<CustomerProductViewModel> products = _customerService.GetProductsByShop(shopName);
            if (products.Any())
            {
                ViewBag.ShopName = products.First().ShopName;
                ViewBag.ShopPhoneNo = products.First().ShopPhoneNo;
                return View(products);
            }
            return Location();
        }
    }
}

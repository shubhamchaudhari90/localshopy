using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{

    public class CustomerController : Controller
    {
        private readonly CustomerService _service;

        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        // GET: /Customer/Products
        public IActionResult Products()
        {
            var selectedLocation = HttpContext.Session.GetString("SelectedLocation");

            if (selectedLocation == null || !_service.IsLocationValid(selectedLocation))
            {
                return NotFound();
            }
            var allProducts = _service.GetAllProducts(selectedLocation);
            return View(allProducts);
        }

        public IActionResult GetLocation()
        {
            var locations = _service.GetAllLocations(); // List<string>
            return View(locations);
        }

        [HttpPost]
        public IActionResult GetLocation(string location)
        {
            HttpContext.Session.SetString("SelectedLocation", location);
            return RedirectToAction("Products");
        }
    }
}

using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly IEncodingService _encodingService;
        private readonly ICustomerService _customerService;

        public CustomerController(ILocationService locationService, IEncodingService encodingService, ICustomerService customerService)
        {
            _locationService = locationService;
            _encodingService = encodingService;
            _customerService = customerService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Location()
        {
            List<Location> locations = await _locationService.GetActiveLocations();
            LocationViewModel model = new LocationViewModel()
            {
                Locations = locations,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Location(LocationViewModel model)
        {

            List<Location> locations = await _locationService.GetActiveLocations();
            if (locations.Any(x => x.Id == model.SelectedLocationId))
            {
                string locationKey = _encodingService.Encode("Location");
                string locationValue = _encodingService.Encode(model.SelectedLocationId.ToString() ?? "");
                HttpContext.Session.SetString(locationKey, locationValue);

                return RedirectToAction("Products");
            }

            return View(locations);
        }

        public async Task<IActionResult> Products()
        {
            Guid location = GetLocationFromSession();
            if (location == Guid.Empty)
            {
                return RedirectToAction(nameof(Location));
            }
            var model = await _customerService.GetProductsByLocation(location);
            return View(model);
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

    }
}

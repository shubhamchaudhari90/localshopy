using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class LocationController : Controller
    {
        private readonly LocationService _service;

        public LocationController(LocationService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            var locations = _service.GetAllLocations();
            return View(locations);
        }

        // CREATE
        [HttpPost]
        public IActionResult Create(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                return RedirectToAction(nameof(Index));
            _service.AddLocation(locationName.Trim());
            return RedirectToAction(nameof(Index));
        }

        // UPDATE
        [HttpPost]
        public IActionResult Edit(string oldName, string newName)
        {
            _service.UpdateLocation(oldName.Trim(), newName.Trim());
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public IActionResult Delete(string name)
        {
            _service.Delete(name.Trim());
            return RedirectToAction(nameof(Index));
        }
    }
}

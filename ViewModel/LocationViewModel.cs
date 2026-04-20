using localshopyNew.Models;

namespace localshopyNew.ViewModel
{
    public class LocationViewModel
    {
        public Guid? SelectedLocationId { get; set; }
        public IEnumerable<Location> Locations { get; set; } = [];
    }

}

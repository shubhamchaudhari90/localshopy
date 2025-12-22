using localshopyNew.Models;
using System.Text.Json;

namespace localshopyNew.Services
{
    public class LocationService
    {
        private readonly string _locationsPath;

        public LocationService(IWebHostEnvironment env)
        {
            _locationsPath = Path.Combine(env.ContentRootPath, "App_Data", "Locations.json");
        }
        private LocationsData ReadLocationsJson()
        {
            if (!File.Exists(_locationsPath))
                return new LocationsData();

            var json = File.ReadAllText(_locationsPath);
            return JsonSerializer.Deserialize<LocationsData>(json) ?? new LocationsData();
        }

        private void WriteLocationsJson(LocationsData data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_locationsPath, json);
        }

        public List<string> GetAllLocations() => ReadLocationsJson().Locations;

        public void AddLocation(string location)
        {
            var data = ReadLocationsJson();
            if (!data.Locations.Contains(location, StringComparer.OrdinalIgnoreCase))
            {
                data.Locations.Add(location);
                WriteLocationsJson(data);
            }
        }

        public void UpdateLocation(string oldLocation, string newLocation)
        {
            var data = ReadLocationsJson();
            var index = data.Locations.FindIndex(l => l.Equals(oldLocation, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                data.Locations[index] = newLocation;
                WriteLocationsJson(data);
            }
        }

        public void Delete(string location)
        {
            var data = ReadLocationsJson();
            if (data.Locations.RemoveAll(l => l.Equals(location, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                WriteLocationsJson(data);
            }
        }


    }
}

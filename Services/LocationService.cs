using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDBContext _context;

        public LocationService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> IsLocationNameExists(string name)
        {
            return await _context.Locations.AnyAsync(x => x.Name == name);
        }

        public async Task<Location?> GetLocationById(Guid id)
        {
            return await _context.Locations.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Location>> GetActiveLocations()
        {
            var locations = await _context.Locations.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
            return locations;
        }

        public async Task<List<Location>> GetInActiveLocations()
        {
            var locations = await _context.Locations.Where(x => !x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
            return locations;
        }

        public async Task<bool> AddLocation(Location location)
        {
            location.Id = Guid.NewGuid();
            int count = _context.Locations.Any() ? _context.Locations.Max(x => x.SortOrder) : 0;

            location.SortOrder = count + 1;
            location.IsActive = true;
            await _context.AddAsync(location);
            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateLocation(Location model)
        {
            var location = await _context.Locations.FindAsync(model.Id);
            if (location == null)
                return false;

            location.Name = model.Name;
            location.SortOrder = model.SortOrder;
            location.IsActive = model.IsActive;

            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
                return true;
            return false;
        }

        public async Task<bool> DeleteLocation(Guid id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                int rowsDeleted = await _context.SaveChangesAsync();
                if (rowsDeleted > 0)
                    return true;
                return false;
            }
            return false;
        }
    }
}

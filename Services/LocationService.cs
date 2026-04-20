using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
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

        // Check if location name exists (read-only, fast)
        public async Task<bool> IsLocationNameExists(string name)
        {
            return await _context.Locations.AsNoTracking().AnyAsync(x => x.Name == name);
        }

        // Get location by Id (read-only)
        public async Task<Location?> GetLocationById(Guid id)
        {
            return await _context.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        // Get active locations
        public async Task<List<Location>> GetActiveLocations()
        {
            return await _context.Locations.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        }

        // Get inactive locations
        public async Task<List<Location>> GetInActiveLocations()
        {
            return await _context.Locations.AsNoTracking().Where(x => !x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        }

        // Add a new location
        public async Task<bool> AddLocation(Location location)
        {
            location.Id = Guid.NewGuid();
            location.SortOrder = (_context.Locations.Max(x => (int?)x.SortOrder) ?? 0) + 1;
            location.IsActive = true;

            await _context.Locations.AddAsync(location);
            var rowsInserted = await _context.SaveChangesAsync();
            return rowsInserted > 0;
        }

        // Update location
        public async Task<bool> UpdateLocation(Location model)
        {
            var location = await _context.Locations.FindAsync(model.Id);
            if (location == null) return false;

            location.Name = model.Name;
            location.SortOrder = model.SortOrder;
            location.IsActive = model.IsActive;

            var rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0;
        }

        // Delete location
        public async Task<bool> DeleteLocation(Guid id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id);
            if (location == null) return false;

            _context.Locations.Remove(location);
            var rowsDeleted = await _context.SaveChangesAsync();
            return rowsDeleted > 0;
        }

        public async Task<List<ShopkeeperNotificationViewModel>> GetShopkeeperTokens()
        {
            return await (

                from shop in _context.Shops
                join device in _context.UserDevices
                    on shop.OwnerEmailId equals device.EmailId
                select new ShopkeeperNotificationViewModel
                {
                    FcmToken = device.FcmToken
                }).Distinct().ToListAsync();
        }
    }
}

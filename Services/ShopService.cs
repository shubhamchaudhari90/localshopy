using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class ShopService : IShopService
    {
        private readonly AppDBContext _context;

        public ShopService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> IsShopNameExists(string name)
        {
            return await _context.Shops.AnyAsync(x => x.Name == name);
        }

        public async Task<Shop?> GetShopById(Guid id)
        {
            return await _context.Shops.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Shop>> GetActiveShops()
        {
            var shops = await _context.Shops.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return shops;
        }

        public async Task<List<Shop>> GetInActiveShops()
        {
            var shops = await _context.Shops.Where(x => !x.IsActive).OrderBy(x => x.Name).ToListAsync();
            return shops;
        }

        public async Task<bool> AddShop(Shop shop)
        {
            shop.Id = Guid.NewGuid();

            await _context.AddAsync(shop);
            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateShop(Shop shop)
        {
            var existingShop = await _context.Shops.FindAsync(shop.Id);
            if (existingShop == null)
                return false;

            shop.CreatedAt = existingShop.CreatedAt;

            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
                return true;
            return false;
        }

        public async Task<bool> DeleteShop(Guid id)
        {
            var category = await _context.Categoties.FirstOrDefaultAsync(x => x.Id == id);
            if (category != null)
            {
                _context.Categoties.Remove(category);
                int rowsDeleted = await _context.SaveChangesAsync();
                if (rowsDeleted > 0)
                    return true;
                return false;
            }
            return false;
        }
    }
}

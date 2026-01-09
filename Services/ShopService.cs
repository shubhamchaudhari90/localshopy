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
            return await _context.Shops.AnyAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> IsShopOwnerEmailExists(string ownerEmailId)
        {
            return await _context.Shops.AnyAsync(x => x.OwnerEmailId.ToLower() == ownerEmailId.ToLower());
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
            var existingShop = await _context.Shops.FirstOrDefaultAsync(x => x.Name.ToLower() == shop.Name.ToLower() || x.OwnerEmailId.ToLower() == shop.OwnerEmailId.ToLower());
            if (existingShop != null)
            {
                return false;
            }
            shop.Id = Guid.NewGuid();
            shop.CreatedAt = DateTime.Now;
            shop.IsActive = true;
            shop.AccountValidTill = DateTime.Today.AddMonths(1);
            shop.OwnerEmailId = shop.OwnerEmailId.ToLower();
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

            if (existingShop.Name.ToLower() != shop.Name.ToLower())
            {
                bool isNameExists = await IsShopNameExists(shop.Name);
                if (isNameExists)
                {
                    return false;
                }
            }

            if (existingShop.OwnerEmailId.ToLower() != shop.OwnerEmailId.ToLower())
            {
                bool isShopExists = await IsShopOwnerEmailExists(shop.OwnerEmailId.ToLower());
                if (isShopExists)
                {
                    return false;
                }
            }

            existingShop.Name = shop.Name;
            existingShop.PhoneNo = shop.PhoneNo;
            existingShop.Address = shop.Address;
            existingShop.OwnerEmailId = shop.OwnerEmailId.ToLower();
            if (!string.IsNullOrEmpty(shop.Password))
                existingShop.Password = shop.Password;
            existingShop.IsOpen = shop.IsOpen;
            existingShop.ServedLocations = shop.ServedLocations;
            existingShop.AccountValidTill = shop.AccountValidTill;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteShop(Guid id)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Id == id);
            if (shop != null)
            {
                _context.Shops.Remove(shop);
                int rowsDeleted = await _context.SaveChangesAsync();
                if (rowsDeleted > 0)
                    return true;
                return false;
            }
            return false;
        }

        public async Task<List<Shop>> GetAllShops()
        {
            var shops = await _context.Shops.OrderBy(x => x.Name).ToListAsync();
            return shops;
        }
    }
}

using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IShopService
    {
        Task<bool> IsShopNameExists(string name);
        Task<Shop?> GetShopById(Guid id);
        Task<List<Shop>> GetActiveShops();
        Task<List<Shop>> GetInActiveShops();
        Task<bool> AddShop(Shop shop);
        Task<bool> UpdateShop(Shop shop);
        Task<bool> DeleteShop(Guid id);
    }
}

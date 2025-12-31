using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IShopkeeperService
    {
        Task<Shop?> GetShopByLoginModel(LoginViewModel model);
        Task<ShopProductsViewModel> GetShopDetailsById(Guid id);
        Task<ShopProductsViewModel> UpdateShopData(Shop shop);

    }
}

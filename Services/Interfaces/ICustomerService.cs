using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ShopProductsViewModel?> GetShopDetailsByName(string shopName);
        Task<List<ProductViewModel>> GetProductsByCategories(string categories);
        Task<List<Categoty>> GetCategoriesByLocation(Guid locationId);
    }
}

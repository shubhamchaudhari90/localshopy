using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ShopProductsViewModel?> GetShopDetailsByName(string shopName);
        Task<ProductViewModel?> GetProductDetailsByName(string shopName, string productName, string emailId);
        Task<List<ProductViewModel>> GetProductsByCategories(string categories);
        Task<List<Categoty>> GetCategoriesByLocation(Guid locationId);
    }
}

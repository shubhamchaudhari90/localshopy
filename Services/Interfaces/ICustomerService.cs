using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ShopProductsViewModel?> GetShopDetailsByName(string shopName, string emailId);
        Task<ProductViewModel?> GetProductDetailsByName(string shopName, string productName, string emailId);
        Task<List<ProductViewModel>?> GetProductsByCategories(string categories, string emailId);
        Task<List<Category>?> GetCategoriesByLocation(Guid locationId);
        Task AddReview(Review review);
    }
}

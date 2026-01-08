using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IShopProductService
    {
        Task<List<ProductViewModel>?> GetAllShopProduct(Guid shopId);
        Task<ProductViewModel?> GetProductDetails(Guid productId);
        Task UpdateProduct(ProductViewModel product);
    }
}

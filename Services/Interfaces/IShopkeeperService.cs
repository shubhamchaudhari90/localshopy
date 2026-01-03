using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IShopkeeperService
    {
        Task<Shop?> GetShopByLoginModel(LoginViewModel model);
        Task<ShopProductsViewModel?> GetShopDetailsById(Guid id);
        Task<ProductViewModel?> GetProductById(Guid id);
        Task<ShopProductsViewModel?> UpdateShopData(Shop shop);
        Task<bool> IsProductValid(Product product);
        Task<bool> AddProductInShop(Product product);
        Task<string?> UpdateProductInShop(Product product);
        Task<bool> DeleteProductFromShop(Guid shopId, Guid productId);
        Task<List<string?>> GetAllImageNames();
    }
}

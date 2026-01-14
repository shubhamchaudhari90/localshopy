using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface ICartService
    {
        Task<bool> AddProductToCart(string productName, string shopName, string emailId);
        Task<bool> RemoveProductFromCart(Guid productId, string emailId);
        Task<List<CartViewModel>> GetCartDetails(string emailId, Guid locationId);

        Task<bool> UpdateCartQuantity(Guid productId, int quantity, string emailId);
    }
}

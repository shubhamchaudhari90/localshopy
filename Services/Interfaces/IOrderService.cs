using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrders(string emailId, Guid locationId);

        Task<Order?> GetOrderById(Guid id);

        Task<List<Order>> GetAllOrdersByShopId(Guid shopId);

        Task<List<Order>> OrdersToServe(Guid shopId);

        Task<List<Order>> PlaceOrder(string emailId, Guid locationId, string flatNumber, string wing, string mobileNumber);

        Task<string> Cancel(Guid id);

        Task<string> Accept(Guid id, Guid shopId);

        Task<string> Reject(Guid id, Guid shopId);

        Task<string> Processing(Guid id, Guid shopId);

        Task<string> OutForDelivery(Guid id, Guid shopId);

        Task<string> Delivered(Guid id, Guid shopId);

        Task<string> PreOrder(Guid id, Guid shopId);

        Task<bool> SaveToken(string emailId, string token, string role);

        Task<List<string>> GetToken(Guid orderID);

        Task<List<ShopkeeperNotificationViewModel>> GetShopkeeperTokens(List<Guid> orderIds);
    }
}

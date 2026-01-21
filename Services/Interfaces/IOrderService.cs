using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrders(string emailId, Guid locationId);

        Task<Order?> GetOrderById(Guid id);

        Task<List<Order>> GetAllOrdersByShopId(Guid shopId);

        Task<List<Order>> OrdersToServe(Guid shopId);

        Task<List<Order>> PlaceOrder(string emailId, Guid locationId, string flatNumber, string wing);

        Task<bool> Cancel(Guid id);

        Task<bool> Accept(Guid id, Guid shopId);

        Task<bool> Reject(Guid id, Guid shopId);

        Task<bool> Processing(Guid id, Guid shopId);

        Task<bool> OutForDelivery(Guid id, Guid shopId);

        Task<bool> Delivered(Guid id, Guid shopId);
    }
}

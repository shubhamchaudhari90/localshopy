using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> PlaceOrder(string emailId, Guid locationId, string flatNumber, string wing);
        Task<List<Order>> GetAllOrders(string emailId, Guid locationId);
        Task<Order?> GetOrder(Guid id);
    }
}

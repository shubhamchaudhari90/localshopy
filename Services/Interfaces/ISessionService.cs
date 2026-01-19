namespace localshopyNew.Services.Interfaces
{
    public interface ISessionService
    {
        void SetLocation(Guid locationId);
        Guid GetLocation();
        void RemoveLocation();

        void SetCartCount(int cartCount);
        int GetCartCount();

        void SetShopId(Guid shopId);
        Guid GetShopId();

        void SetProductId(Guid productId);
        Guid GetProductId();

        void SetString(string key, string value);


        void Logout();
    }
}

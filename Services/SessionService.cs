using localshopyNew.Constants;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace localshopyNew.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncodingService _encodingService;

        public SessionService(
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider provider,
            IEncodingService encodingService)
        {
            _httpContextAccessor = httpContextAccessor;
            _encodingService = encodingService;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public void SetLocation(Guid locationId)
        {
            string locationKey = _encodingService.Encode("Location");
            string locationValue = _encodingService.Encode(locationId.ToString());
            Session.SetString(locationKey, locationValue);
        }

        public Guid GetLocation()
        {
            string locationKey = _encodingService.Encode("Location");
            string? encodedLocation = Session.GetString(locationKey);
            if (string.IsNullOrEmpty(encodedLocation))
            {
                return Guid.Empty;
            }
            string locationValue = _encodingService.Decode(encodedLocation ?? string.Empty);
            Guid location = Guid.Parse(locationValue);
            return location;
        }

        public void RemoveLocation()
        {
            string locationKey = _encodingService.Encode("Location");
            Session.Remove(locationKey);
        }

        public void SetCartCount(int cartCount)
        {
            string cartCountKey = _encodingService.Encode("CartCount");
            string cartCountValue = _encodingService.Encode(cartCount.ToString());
            Session.SetString(cartCountKey, cartCountValue);
        }

        public int GetCartCount()
        {
            string cartCountKey = _encodingService.Encode("CartCount");
            string? encodedCartCount = Session.GetString(cartCountKey);
            int cartCount = 0;
            if (string.IsNullOrEmpty(encodedCartCount))
            {
                return cartCount;
            }
            string cartCountValue = _encodingService.Decode(encodedCartCount ?? string.Empty);
            if (!int.TryParse(cartCountValue, out cartCount))
                return cartCount;
            return cartCount;
        }

        public void SetShopId(Guid shopId)
        {
            string shopIdKey = _encodingService.Encode("ShopId");
            string shopIdValue = _encodingService.Encode(shopId.ToString());
            Session.SetString(shopIdKey, shopIdValue);
        }

        public Guid GetShopId()
        {
            string shopIdKey = _encodingService.Encode("ShopId");
            string? encodedShopId = Session.GetString(shopIdKey);
            if (string.IsNullOrEmpty(encodedShopId))
            {
                return Guid.Empty;
            }
            string shopIdValue = _encodingService.Decode(encodedShopId ?? string.Empty);
            Guid shopId = Guid.Parse(shopIdValue);
            return shopId;
        }

        public void SetProductId(Guid productId)
        {
            string productIdKey = _encodingService.Encode("ProductId");
            string productIdValue = _encodingService.Encode(productId.ToString());

            Session.SetString(productIdKey, productIdValue);
        }

        public Guid GetProductId()
        {
            string productIdKey = _encodingService.Encode("ProductId");
            string? encodedProductId = Session.GetString(productIdKey);
            if (string.IsNullOrEmpty(encodedProductId))
            {
                return Guid.Empty;
            }
            string productIdValue = _encodingService.Decode(encodedProductId ?? string.Empty);
            Guid productId = Guid.Parse(productIdValue);
            return productId;
        }

        public void SetString(string key, string value)
        {
            Session.SetString(key, value);
        }

        public void Logout()
        {
            string shopIdKey = _encodingService.Encode("ShopId");
            Session.Remove(shopIdKey);
            Session.Remove(RoleConstant.IsShopkeeper);
            Session.Remove(RoleConstant.Admin);
            Session.Remove(RoleConstant.Shopkeeper);
            Session.Clear();
        }
    }
}

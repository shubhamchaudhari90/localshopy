using localshopyNew.Models;

namespace localshopyNew.ViewModels
{
    public class CustomerProductViewModel
    {
        public required string ShopName { get; set; }
        public required string ShopPhoneNo { get; set; }
        public string? CategotyName { get; set; }
        public required ShopProduct Product { get; set; }
    }
}

using localshopyNew.Models;

namespace localshopyNew.ViewModel
{
    public class ShopProductsViewModel
    {
        public Shop? Shop { get; set; }
        public List<string>? Locations { get; set; }
        public List<ProductViewModel>? Products { get; set; }

    }
}

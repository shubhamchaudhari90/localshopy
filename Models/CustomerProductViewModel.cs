namespace localshopyNew.Models
{
    public class CustomerProductViewModel
    {
        public required string ShopName { get; set; }
        public required string ShopPhoneNo { get; set; }
        public string CategotyName { get; set; }
        public required Product Product { get; set; }
    }
}

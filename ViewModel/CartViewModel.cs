namespace localshopyNew.ViewModel
{
    public class CartViewModel
    {
        public Guid ShopId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductMasterId { get; set; }
        public Guid CategoryId { get; set; }


        public string ShopName { get; set; }
        public string ProductName { get; set; }
        public string? ImageFileName { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int FinalPrice
        {
            get
            {
                var now = DateTime.Today;

                if (Discount <= 0 || DiscountValidFrom == null || DiscountValidFrom == null ||
                    now < DiscountValidFrom || now > DiscountValidTill)
                {
                    return Price;
                }

                var discountAmount = Price * (Discount / 100);
                return (int)Math.Round(Price - discountAmount);
            }
        }


        public double Discount { get; set; }
        public DateTime? DiscountValidFrom { get; set; }
        public DateTime? DiscountValidTill { get; set; }
        public string CategoryName { get; set; }
        public string Type { get; set; }

        public int ShopNumber { get; set; }
    }
}

namespace localshopyNew.Models
{
    public class Product
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageFileName { get; set; }
        public double Discount { get; set; } // percentage (e.g. 10 = 10%)
        public DateTime? DiscountValidFrom { get; set; }
        public DateTime? DiscountValidTill { get; set; }
        public int FinalPrice
        {
            get
            {
                var now = DateTime.UtcNow;

                if (Discount <= 0 || DiscountValidFrom == null || DiscountValidFrom == null ||
                    now < DiscountValidFrom || now > DiscountValidTill)
                {
                    return Price;
                }

                var discountAmount = Price * (Discount / 100);
                return (int)Math.Round(Price - discountAmount);
            }
        }
    }
}

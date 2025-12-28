namespace localshopyNew.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid ProductMasterId { get; set; }
        public Guid ShopId { get; set; }
        public int SortOrder { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageFileName { get; set; }
        public double Discount { get; set; } // percentage (e.g. 10 = 10%)
        public DateTime? DiscountValidFrom { get; set; }
        public DateTime? DiscountValidTill { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        //[NotMapped]
        //public int FinalPrice
        //{
        //    get
        //    {
        //        var now = DateTime.UtcNow;

        //        if (Discount <= 0 || DiscountValidFrom == null || DiscountValidFrom == null ||
        //            now < DiscountValidFrom || now > DiscountValidTill)
        //        {
        //            return Price;
        //        }

        //        var discountAmount = Price * (Discount / 100);
        //        return (int)Math.Round(Price - discountAmount);
        //    }
        //}
    }
}

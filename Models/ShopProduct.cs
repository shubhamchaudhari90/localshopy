using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class ShopProduct
    {
        public int Id { get; set; }

        public int ShopId { get; set; }

        public Shop Shop { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        public required decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public string? ImageFileName { get; set; }


        [Range(5, 95, ErrorMessage = "Discount must be between 5% and 95%.")]
        public int Discount { get; set; } = 0;

        public DateTime? DiscountValidFrom { get; set; }

        public DateTime? DiscountValidTill { get; set; }

        public decimal FinalPrice
        {
            get
            {
                var now = DateTime.UtcNow;

                if (Discount <= 0 || DiscountValidFrom == null || DiscountValidTill == null ||
                    now < DiscountValidFrom || now > DiscountValidTill)
                {
                    return Math.Round(Price, 2);
                }

                decimal discountAmount = Price * (Discount / 100m); // note the 100m
                return Math.Round(Price - discountAmount, 2, MidpointRounding.AwayFromZero);
            }
        }

        public bool IsActive { get; set; } = true;
    }
}

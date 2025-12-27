using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace localshopyNew.Models
{
    public class ShopProduct
    {
        public int Id { get; set; }

        public int ShopId { get; set; }
        public Shop Shop { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        public string? ImageFileName { get; set; }

        [Range(0, 95)]
        public int Discount { get; set; } = 0;

        public DateTime? DiscountValidFrom { get; set; }
        public DateTime? DiscountValidTill { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public decimal FinalPrice
        {
            get
            {
                var now = DateTime.UtcNow;

                if (Discount <= 0 ||
                    DiscountValidFrom == null ||
                    DiscountValidTill == null ||
                    now < DiscountValidFrom ||
                    now > DiscountValidTill)
                {
                    return Math.Round(Price, 2);
                }

                var discountAmount = Price * (Discount / 100m);
                return Math.Round(Price - discountAmount, 2, MidpointRounding.AwayFromZero);
            }
        }
    }

}

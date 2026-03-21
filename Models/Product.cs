using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace localshopyNew.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public Guid ShopId { get; set; }

        public Guid ProductMasterId { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        [RegularExpression("Veg|Non-Veg|Egg", ErrorMessage = "Type must be Veg, Non-Veg, or Egg.")]
        public required string Type { get; set; } = "Non-Veg";

        public int Price { get; set; }

        public string? ImageFileName { get; set; }

        public double Discount { get; set; }

        public DateTime? DiscountValidFrom { get; set; }

        public DateTime? DiscountValidTill { get; set; }

        public required DateTime CreatedAt { get; set; } =
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));

        public DateTime UpdatedAt { get; set; }

        public int SortOrder { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public IFormFile? ProductImage { get; set; }
    }
}
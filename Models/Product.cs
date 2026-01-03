using System.ComponentModel.DataAnnotations.Schema;

namespace localshopyNew.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid ProductMasterId { get; set; }
        public Guid ShopId { get; set; }
        public int SortOrder { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageFileName { get; set; }
        public double Discount { get; set; } // percentage (e.g. 10 = 10%)
        public DateTime? DiscountValidFrom { get; set; }
        public DateTime? DiscountValidTill { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public IFormFile? ProductImage { get; set; }
    }
}

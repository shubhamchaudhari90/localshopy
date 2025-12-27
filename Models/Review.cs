using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ShopProductId { get; set; }
        public ShopProduct ShopProduct { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Reviewer { get; set; } = string.Empty;

        [Range(0, 5)]
        public decimal Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

}

using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Shop
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string PhoneNo { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string OwnerEmailId { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsOpen { get; set; } = true;

        public DateTime AccountValidTill { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ShopProduct> Products { get; set; } = new List<ShopProduct>();
        public ICollection<ShopLocation> ShopLocations { get; set; } = new List<ShopLocation>();
    }

}

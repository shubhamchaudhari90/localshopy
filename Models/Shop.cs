using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Shop
    {
        public string? Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
        public required string PhoneNo { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string OwnerEmailId { get; set; }

        public required string Password { get; set; }

        public bool IsOpen { get; set; } = true;

        public List<Product> Products { get; set; } = new();
        public List<string> ServedLocations { get; set; } = new();

        public List<ShopProduct> Products { get; set; } = [];

        public List<Location> ServedLocations { get; set; } = [];

        public DateTime AccountValidTill { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
>>>>>>> Stashed changes
    }
}

using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Shop
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$",
        ErrorMessage = "Name can only contain letters, numbers and spaces.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$",
        ErrorMessage = "Phone number must be exactly 10 digits.")]
        public required string PhoneNo { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string OwnerEmailId { get; set; }

        public required string Password { get; set; }

        public bool IsOpen { get; set; } = true;

        public List<Guid> ServedLocations { get; set; } = [];

        public DateTime AccountValidTill { get; set; } = DateTime.Now;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
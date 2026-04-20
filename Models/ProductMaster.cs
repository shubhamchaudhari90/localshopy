using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class ProductMaster
    {
        public Guid Id { get; set; }

        public required Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9_ ()]+$",
        ErrorMessage = "Name can only contain letters, numbers, underscores, and spaces.")]
        public required string ProductName { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

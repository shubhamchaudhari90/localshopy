using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z0-9_ ]+$",
        ErrorMessage = "Name can only contain letters, numbers, underscores, and spaces.")]
        public required string Name { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

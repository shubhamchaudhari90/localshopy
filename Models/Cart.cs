using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string EmailId { get; set; }

        public required Guid ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public required int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

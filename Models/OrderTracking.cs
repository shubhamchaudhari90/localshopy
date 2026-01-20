using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class OrderTracking
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public required Guid OrderId { get; set; }

        public required string Status { get; set; }

        public required DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

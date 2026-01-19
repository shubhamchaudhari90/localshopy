using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace localshopyNew.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; }


        [Required]
        [MaxLength(200)]
        public string Type { get; set; }

        public int UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int TotalPrice { get; set; }

        // Navigation Property
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }
    }

}

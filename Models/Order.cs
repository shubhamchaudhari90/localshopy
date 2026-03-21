using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string EmailId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        public Guid ShopId { get; set; }

        [Required]
        [MaxLength(200)]

        public string ShopName { get; set; }

        [StringLength(10, MinimumLength = 10)]
        public string ShopContactNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";
        // PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED, REJECTED

        public bool IsPreOrder { get; set; } = false;

        public int Subtotal { get; set; }

        public int Tax { get; set; }

        public int ShippingFee { get; set; } = 0;

        public int TotalAmount { get; set; }

        [MaxLength(20)]
        public string PaymentMethod { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "UNPAID";

        [StringLength(10, MinimumLength = 10)]
        public string CustomerMobileNumber { get; set; }

        public string FlatNumber { get; set; }

        public string Wing { get; set; }

        public string Society { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        public string BillingAddress { get; set; }

        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        public DateTime UpdatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));

        // Navigation Property
        public ICollection<OrderItem> OrderItems { get; set; }
    }

}

namespace localshopyNew.Models
{
    public class OrderTracking
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public required string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

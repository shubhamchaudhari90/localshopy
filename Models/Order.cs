namespace localshopyNew.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductMasterId { get; set; }

        public int Count { get; set; }

        public decimal ProductPrice { get; set; }

        public required string Status { get; set; }
    }
}

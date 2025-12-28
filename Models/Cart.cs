namespace localshopyNew.Models
{
    public class Cart
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ProductMasterId { get; set; }
        public required string Count { get; set; }
        public decimal ProductPrice { get; set; }
    }
}

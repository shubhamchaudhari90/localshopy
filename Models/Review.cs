namespace localshopyNew.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public required string Reviewer { get; set; }
        public required string Comment { get; set; }
        public decimal Rating { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsApproved { get; set; }
    }
}

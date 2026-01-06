namespace localshopyNew.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public required string Reviewer { get; set; }
        public string? Comment { get; set; }
        public decimal Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
    }
}

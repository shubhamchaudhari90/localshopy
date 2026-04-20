namespace localshopyNew.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public required string Reviewer { get; set; }
        public string? Comment { get; set; }
        public decimal Rating { get; set; }
        public required DateTime CreatedAt { get; set; } =
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
    }
}

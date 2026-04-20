using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class BlockedUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string EmailId { get; set; }
        public string Reason { get; set; }
        public string SuggestedBy { get; set; }
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        public DateTime UpdatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
    }
}
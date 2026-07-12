namespace localshopyNew.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
    }
}

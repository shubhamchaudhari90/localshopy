namespace localshopyNew.Models
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public required string EmailId { get; set; }
        public string Role { get; set; } = "User";
        public required string FcmToken { get; set; }
    }
}

namespace localshopyNew.Models
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public string EmailId { get; set; }
        public string Role { get; set; } = "User";
        public string FcmToken { get; set; }
    }
}

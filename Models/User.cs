namespace localshopyNew.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public required string EmailId { get; set; }
    }
}

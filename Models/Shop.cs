namespace localshopyNew.Models
{
    public class Shop
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string PhoneNo { get; set; }
        public required string OwnerEmailId { get; set; }
        public required string Password { get; set; }
        public bool IsOpen { get; set; } = true;
        public List<Product> Products { get; set; } = new();
        public List<string> ServedLocations { get; set; } = new();
    }
}

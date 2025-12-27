namespace localshopyNew.Models
{
    public class ProductMaster
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<string> ProductNames { get; set; } = new();
    }
}

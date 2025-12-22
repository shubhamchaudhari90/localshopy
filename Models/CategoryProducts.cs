namespace localshopyNew.Models
{
    public class CategoryProducts
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<string> ProductName { get; set; } = new();
    }

}

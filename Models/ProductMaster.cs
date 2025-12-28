namespace localshopyNew.Models
{
    public class ProductMaster
    {
        public Guid Id { get; set; }
        public required Guid CategoryId { get; set; }
        public required string ProductName { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

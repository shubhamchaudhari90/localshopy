namespace localshopyNew.Models
{
    public class Categoty
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

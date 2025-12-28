namespace localshopyNew.Models
{
    public class Location
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

}

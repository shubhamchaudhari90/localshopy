using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<ShopLocation> ShopLocations { get; set; } = new List<ShopLocation>();
    }

}

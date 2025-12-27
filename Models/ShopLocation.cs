namespace localshopyNew.Models
{
    public class ShopLocation
    {
        public int ShopId { get; set; }
        public Shop Shop { get; set; } = null!;

        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;
    }

}

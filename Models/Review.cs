using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Review
    {
        public int Id { get; set; }

        public required string ShopProductId { get; set; }

        public required string Reviewer { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Range(0, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsReviewed { get; set; } = false;

        public bool IsActive { get; set; }
    }
}

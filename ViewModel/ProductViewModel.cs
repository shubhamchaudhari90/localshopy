using localshopyNew.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace localshopyNew.ViewModel
{
    public class ProductViewModel : Product
    {
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category")]
        public Guid CategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        public IEnumerable<SelectListItem>? ProductMasters { get; set; }

        public int FinalPrice
        {
            get
            {
                var now = DateTime.Today;

                if (Discount <= 0 || DiscountValidFrom == null || DiscountValidFrom == null ||
                    now < DiscountValidFrom || now > DiscountValidTill)
                {
                    return Price;
                }

                var discountAmount = Price * (Discount / 100);
                return (int)Math.Round(Price - discountAmount);
            }
        }

        public string? CategoryName { get; set; }
        public string? ShopName { get; set; }

        public string? ProductMasterName { get; set; }

        public List<Review>? Reviews { get; set; }

        public bool IsReviewed { get; set; }

        public int ReviewCount { get; set; }

        public decimal AverageRating { get; set; }
    }
}

using localshopyNew.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace localshopyNew.ViewModel
{
    public class ProductViewModel : Product
    {
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category")]
        public Guid CategoryId { get; set; }  // nullable because "--Select--" has empty value

        public IEnumerable<SelectListItem> Categories { get; set; }

        public IEnumerable<SelectListItem> ProductMasters { get; set; }

        public int FinalPrice
        {
            get
            {
                var now = DateTime.UtcNow;

                if (Discount <= 0 || DiscountValidFrom == null || DiscountValidFrom == null ||
                    now < DiscountValidFrom || now > DiscountValidTill)
                {
                    return Price;
                }

                var discountAmount = Price * (Discount / 100);
                return (int)Math.Round(Price - discountAmount);
            }
        }
    }
}

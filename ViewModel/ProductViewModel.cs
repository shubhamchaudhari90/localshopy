using localshopyNew.Models;

namespace localshopyNew.ViewModel
{
    public class ProductViewModel : Product
    {
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

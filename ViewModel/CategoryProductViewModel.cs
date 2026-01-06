using localshopyNew.Models;

namespace localshopyNew.ViewModel
{
    public class CategoryProductViewModel
    {
        public Category Categoty { get; set; }
        public List<ProductMaster> ProductMasters { get; set; } = [];
    }
}

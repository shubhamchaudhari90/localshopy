using localshopyNew.Models;
using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IProductMasterService
    {
        Task<bool> IsProductNameExists(string name);
        Task<ProductMaster?> GetProductById(Guid id);
        Task<List<CategoryProductViewModel>> GetActiveProducts();
        Task<List<CategoryProductViewModel>> GetInActiveProducts();
        Task<bool> AddProduct(ProductMaster Product);
        Task<bool> UpdateProduct(ProductMaster Product);
        Task<bool> DeleteProduct(Guid id);
    }
}

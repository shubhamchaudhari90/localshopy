using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<bool> IsCategoryNameExists(string name);
        Task<Category?> GetCategoryById(Guid id);
        Task<List<ProductMaster>> GetProductsByCategoryId(Guid id);
        Task<List<Category>> GetActiveCategories();
        Task<List<Category>> GetInActiveCategories();
        Task<bool> AddCategory(Category category);
        Task<bool> UpdateCategory(Category category);
        Task<bool> DeleteCategory(Guid id);
    }
}

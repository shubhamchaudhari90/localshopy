using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<bool> IsCategoryNameExists(string name);
        Task<Categoty?> GetCategoryById(Guid id);
        Task<List<Categoty>> GetActiveCategories();
        Task<List<Categoty>> GetInActiveCategories();
        Task<bool> AddCategory(Categoty category);
        Task<bool> UpdateCategory(Categoty category);
        Task<bool> DeleteCategory(Guid id);
    }
}

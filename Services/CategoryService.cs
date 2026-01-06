using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDBContext _context;

        public CategoryService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCategoryNameExists(string name)
        {
            return await _context.Categoties.AnyAsync(x => x.Name == name);
        }

        public async Task<Category?> GetCategoryById(Guid id)
        {
            return await _context.Categoties.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ProductMaster>> GetProductsByCategoryId(Guid id)
        {
            return await _context.ProductMasters.Where(x => x.CategoryId == id && x.IsActive).ToListAsync();
        }

        public async Task<List<Category>> GetActiveCategories()
        {
            var categories = await _context.Categoties.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
            return categories;
        }

        public async Task<List<Category>> GetInActiveCategories()
        {
            var categories = await _context.Categoties.Where(x => !x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
            return categories;
        }

        public async Task<bool> AddCategory(Category category)
        {
            category.Id = Guid.NewGuid();
            int count = _context.Categoties.Any() ? _context.Categoties.Max(x => x.SortOrder) : 0;
            category.SortOrder = count + 1;
            category.IsActive = true;
            await _context.AddAsync(category);
            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateCategory(Category model)
        {
            var category = await _context.Categoties.FindAsync(model.Id);
            if (category == null)
                return false;

            category.Name = model.Name;
            category.SortOrder = model.SortOrder;
            category.IsActive = model.IsActive;

            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
                return true;
            return false;
        }

        public async Task<bool> DeleteCategory(Guid id)
        {
            var category = await _context.Categoties.FirstOrDefaultAsync(x => x.Id == id);
            if (category != null)
            {
                _context.Categoties.Remove(category);
                int rowsDeleted = await _context.SaveChangesAsync();
                if (rowsDeleted > 0)
                    return true;
                return false;
            }
            return false;
        }
    }
}

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

        // Check if category name exists (read-only, fast)
        public async Task<bool> IsCategoryNameExists(string name)
        {
            return await _context.Categoties.AsNoTracking().AnyAsync(x => x.Name == name);
        }

        // Get category by Id (read-only)
        public async Task<Category?> GetCategoryById(Guid id)
        {
            return await _context.Categoties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        // Get products in a category (read-only)
        public async Task<List<ProductMaster>> GetProductsByCategoryId(Guid id)
        {
            return await _context.ProductMasters.AsNoTracking().Where(x => x.CategoryId == id && x.IsActive).ToListAsync();
        }

        // Get active categories
        public async Task<List<Category>> GetActiveCategories()
        {
            return await _context.Categoties.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        }

        // Get inactive categories
        public async Task<List<Category>> GetInActiveCategories()
        {
            return await _context.Categoties.AsNoTracking().Where(x => !x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        }

        // Add category
        public async Task<bool> AddCategory(Category category)
        {
            category.Id = Guid.NewGuid();
            category.SortOrder = (_context.Categoties.Max(x => (int?)x.SortOrder) ?? 0) + 1;
            category.IsActive = true;

            await _context.Categoties.AddAsync(category);
            var rowsInserted = await _context.SaveChangesAsync();
            return rowsInserted > 0;
        }

        // Update category
        public async Task<bool> UpdateCategory(Category model)
        {
            var category = await _context.Categoties.FindAsync(model.Id);
            if (category == null)
                return false;

            category.Name = model.Name;
            category.SortOrder = model.SortOrder;
            category.IsActive = model.IsActive;

            var rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0;
        }

        // Delete category
        public async Task<bool> DeleteCategory(Guid id)
        {
            var category = await _context.Categoties.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null) return false;

            _context.Categoties.Remove(category);
            var rowsDeleted = await _context.SaveChangesAsync();
            return rowsDeleted > 0;

        }
    }
}
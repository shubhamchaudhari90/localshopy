using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class ProductMasterService : IProductMasterService
    {
        private readonly AppDBContext _context;

        public ProductMasterService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> IsProductNameExists(string name)
        {
            return await _context.ProductMasters.AnyAsync(x => x.ProductName == name);
        }

        public async Task<ProductMaster?> GetProductById(Guid id)
        {
            return await _context.ProductMasters.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CategoryProductViewModel>> GetActiveProducts()
        {
            List<CategoryProductViewModel> products = new List<CategoryProductViewModel>();
            var categories = await _context.Categoties.OrderBy(x => x.SortOrder).ToListAsync();
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    CategoryProductViewModel categoryProduct = new CategoryProductViewModel();
                    categoryProduct.Categoty = category;
                    categoryProduct.ProductMasters.AddRange(await _context.ProductMasters.Where(x => x.IsActive && x.CategoryId == category.Id).OrderBy(x => x.SortOrder).ToListAsync());
                    products.Add(categoryProduct);
                }
            }
            return products;
        }

        public async Task<List<CategoryProductViewModel>> GetInActiveProducts()
        {
            List<CategoryProductViewModel> products = new List<CategoryProductViewModel>();
            var categories = await _context.Categoties.OrderBy(x => x.SortOrder).ToListAsync();
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    CategoryProductViewModel categoryProduct = new CategoryProductViewModel();
                    categoryProduct.Categoty = category;
                    categoryProduct.ProductMasters.AddRange(await _context.ProductMasters.Where(x => !x.IsActive && x.CategoryId == category.Id).OrderBy(x => x.SortOrder).ToListAsync());
                    products.Add(categoryProduct);
                }
            }
            return products;
        }

        public async Task<bool> AddProduct(ProductMaster product)
        {
            product.Id = Guid.NewGuid();
            int count = _context.ProductMasters.Any() ? _context.ProductMasters.Max(x => x.SortOrder) : 0;

            product.SortOrder = count + 1;
            product.IsActive = true;
            await _context.AddAsync(product);
            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateProduct(ProductMaster model)
        {
            var product = await _context.ProductMasters.FindAsync(model.Id);
            if (product == null)
                return false;

            product.ProductName = model.ProductName;
            product.SortOrder = model.SortOrder;
            product.IsActive = model.IsActive;

            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
                return true;
            return false;
        }

        public async Task<bool> DeleteProduct(Guid id)
        {
            var product = await _context.ProductMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                _context.ProductMasters.Remove(product);
                int rowsDeleted = await _context.SaveChangesAsync();
                if (rowsDeleted > 0)
                    return true;
                return false;
            }
            return false;
        }
    }
}

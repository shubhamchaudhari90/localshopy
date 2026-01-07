using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDBContext _context;

        public CustomerService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>?> GetProductsByCategories(string categories)
        {
            List<ProductViewModel> products = new List<ProductViewModel>();

            if (string.IsNullOrEmpty(categories))
            {
                return products;
            }
            List<string> categoryList = categories.Split(", ").ToList();

            products = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops
                on p.ShopId equals shop.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.IsActive    // NOT DELETED
                && p.IsAvailable    // AVAILABLE ONLY (NO OUT OF STOCK)
                && shop.IsOpen      // SHOP SHOULD BE OPEN
                && shop.AccountValidTill.Date >= DateTime.Today // SHOP ACCOUNT SHOULD BE VALID
                && c.IsActive
                && categoryList.Contains(c.Name)

                orderby p.CreatedAt

                select new ProductViewModel
                {
                    Id = p.Id,
                    ProductMasterId = pm.Id,
                    ShopId = p.ShopId,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    ImageFileName = p.ImageFileName,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    ShopName = shop.Name,
                    Type = p.Type
                }).ToListAsync();

            if (products != null && products.Count > 0)
            {
                products = await MapReviewData(products);
            }
            return products;
        }

        public async Task<List<Category>?> GetCategoriesByLocation(Guid locationId)
        {
            var categories = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops
                on p.ShopId equals shop.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.IsActive    // NOT DELETED
                && p.IsAvailable    // AVAILABLE ONLY (NO OUT OF STOCK)
                && shop.IsOpen      // SHOP SHOULD BE OPEN
                && shop.AccountValidTill.Date >= DateTime.Today // SHOP ACCOUNT SHOULD BE VALID
                && shop.ServedLocations.Contains(locationId) // SERVED LOCATION BY SHOP
                && c.IsActive

                //orderby c.Name

                select new Category
                {
                    Name = c.Name,
                    SortOrder = c.SortOrder
                }).Distinct().ToListAsync();

            if (categories != null && categories.Count > 0)
                categories = categories.OrderBy(c => Guid.NewGuid()).ToList();

            return categories;
        }

        public async Task<ShopProductsViewModel?> GetShopDetailsByName(string shopName)
        {
            List<ProductViewModel>? products = await (
                from p in _context.Products

                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty()   // LEFT JOIN

                join s in _context.Shops
                on p.ShopId equals s.Id

                where s.Name == shopName              // case-insensitive by DB collation
                && p.IsActive                     // product active
                && s.IsActive                     // shop active
                && s.AccountValidTill >= DateTime.Today
                && pm.IsActive
                && (c == null || c.IsActive)      // SAFE left join condition

                orderby p.SortOrder

                select new ProductViewModel
                {
                    ShopName = s.Name,

                    Id = p.Id,
                    ProductMasterId = pm.Id,
                    ShopId = p.ShopId,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    ImageFileName = p.ImageFileName,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    Type = string.IsNullOrEmpty(p.Type) ? "VEG" : p.Type
                }).ToListAsync();

            if (products != null && products.Any())
                products = await MapReviewData(products);

            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Name.ToLower() == shopName.ToLower());

            List<string> locations = new List<string>();

            if (shop != null)
            {
                locations = await _context.Locations.Where(x => shop.ServedLocations.Contains(x.Id)).Select(x => x.Name).ToListAsync();
            }

            return new ShopProductsViewModel
            {
                Shop = shop,
                Locations = locations,
                Products = products
            };
        }

        public async Task<ProductViewModel?> GetProductDetailsByName(string shopName, string productName, string emailId)
        {
            Shop? shop = await _context.Shops.FirstOrDefaultAsync(x => x.Name.ToLower() == shopName.ToLower() && x.IsOpen && x.IsActive);
            if (shop == null) { return null; }

            ProductMaster? productMaster = await _context.ProductMasters.FirstOrDefaultAsync(x => x.ProductName.ToLower() == productName.ToLower() && x.IsActive);
            if (productMaster == null) { return null; }

            Category? category = await _context.Categoties.FirstOrDefaultAsync(x => x.Id == productMaster.CategoryId && x.IsActive);
            if (category == null) { return null; }


            Product? product = await _context.Products.FirstOrDefaultAsync(x => x.ShopId == shop.Id && x.ProductMasterId == productMaster.Id && x.IsAvailable && x.IsActive);
            if (product == null) { return null; }

            int reviewCount = await _context.Reviews.Where(x => x.ProductId == product.Id && x.IsApproved && !x.IsRejected).CountAsync();

            var reviews = await _context.Reviews
                .Where(x => x.ProductId == product.Id && x.IsApproved && !x.IsRejected)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            reviews.ForEach(r =>
            {
                r.Reviewer = MaskReviewerEmail(r.Reviewer);
            });

            bool isReviewed = false;

            if (!string.IsNullOrEmpty(emailId))
            {
                isReviewed = await _context.Reviews.AnyAsync(x => x.ProductId == product.Id && x.Reviewer.ToLower() == emailId.ToLower());
            }

            ProductViewModel model = new ProductViewModel()
            {
                Id = product.Id,
                CategoryName = category.Name,
                Description = product.Description,
                Discount = product.Discount,
                DiscountValidFrom = product.DiscountValidFrom,
                DiscountValidTill = product.DiscountValidTill,
                ImageFileName = product.ImageFileName,
                IsActive = product.IsActive,
                Price = product.Price,
                ProductImage = product.ProductImage,
                ProductMasterName = productMaster.ProductName,
                Reviews = reviews,
                ShopName = shop.Name,
                Type = product.Type,
                IsReviewed = isReviewed,
                ReviewCount = reviewCount
            };

            var reviewRatings = await AverageRatingsForProducts(product.Id.ToString());
            if (reviewRatings != null && reviewRatings.Count > 0 && reviewRatings[0] != null && reviewRatings[0].AverageRatings != null)
            {
                model.AverageRating = reviewRatings[0].AverageRatings;
                model.ReviewCount = reviews.Count;
            }
            return model;
        }

        public async Task AddReview(Review review)
        {

            Review entity = new Review()
            {
                Reviewer = review.Reviewer,
                Comment = review.Comment,
                CreatedAt = DateTime.Now,
                Id = review.Id,
                IsApproved = false,
                IsRejected = false,
                ProductId = review.ProductId,
                Rating = review.Rating
            };

            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task<List<ReviewViewModel>> AverageRatingsForProducts(string productIds)
        {
            if (string.IsNullOrWhiteSpace(productIds))
                return new List<ReviewViewModel>();

            var productGuidIds = productIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g.Value)
                .ToList();

            if (!productGuidIds.Any())
                return new List<ReviewViewModel>();

            List<ReviewViewModel> productRatings = new List<ReviewViewModel>();

            productRatings = await _context.Reviews
                .Where(r => r.IsApproved && productGuidIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new ReviewViewModel
                {
                    ProductId = g.Key,
                    ReviewCount = g.Count(),
                    AverageRatings = g.Average(r => (double?)r.Rating) ?? 0
                })
                .ToListAsync();

            return productRatings;
        }

        private string MaskReviewerEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 6)
            {
                string maskedlocalPart = new string('*', localPart.Length);
                return $"{maskedlocalPart}@{domain}";
            }

            var start = localPart.Substring(0, 3);
            var end = localPart.Substring(localPart.Length - 3, 3);
            var masked = new string('*', localPart.Length - 6);

            return $"{start}{masked}{end}@{domain}";
        }

        private async Task<List<ProductViewModel>> MapReviewData(List<ProductViewModel> products)
        {
            if (products != null && products.Count > 0)
            {
                products = products.OrderBy(p => Guid.NewGuid()).ToList();

                var productIdsCsv = string.Join(",", products.Select(x => x.Id));
                if (!string.IsNullOrEmpty(productIdsCsv))
                {
                    List<ReviewViewModel>? averageRatings = await AverageRatingsForProducts(productIdsCsv);
                    if (averageRatings != null && averageRatings.Count > 0)
                    {
                        for (int index = 0; index < products.Count; index++)
                        {
                            var productAverageRating = averageRatings.FirstOrDefault(x => x.ProductId == products[index].Id);
                            if (productAverageRating != null)
                            {
                                products[index].AverageRating = (double)(productAverageRating?.AverageRatings == null ? 0 : productAverageRating.AverageRatings);
                                products[index].ReviewCount = productAverageRating?.ReviewCount == null ? 0 : productAverageRating.ReviewCount;
                            }
                        }
                    }
                }
            }

            return products;
        }
    }
}

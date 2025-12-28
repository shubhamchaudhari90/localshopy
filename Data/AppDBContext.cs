using localshopyNew.Models;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        // Add a DbSet property for each model you want in the database
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Categoty> Categoties { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderTracking> OrderTrackings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductMaster> ProductMasters { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<User> Users { get; set; }
    }
}

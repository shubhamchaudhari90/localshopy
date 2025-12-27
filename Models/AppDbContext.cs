using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<ShopLocation> ShopLocations => Set<ShopLocation>();
        public DbSet<ShopProduct> ShopProducts => Set<ShopProduct>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Shop ↔ Location (Many-to-Many)
            modelBuilder.Entity<ShopLocation>()
                .HasKey(sl => new { sl.ShopId, sl.LocationId });

            modelBuilder.Entity<ShopLocation>()
                .HasOne(sl => sl.Shop)
                .WithMany(s => s.ShopLocations)
                .HasForeignKey(sl => sl.ShopId);

            modelBuilder.Entity<ShopLocation>()
                .HasOne(sl => sl.Location)
                .WithMany(l => l.ShopLocations)
                .HasForeignKey(sl => sl.LocationId);

            // Shop → Products
            modelBuilder.Entity<ShopProduct>()
                .HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId);

            // Product → Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ShopProduct)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ShopProductId);

            // Decimal precision
            modelBuilder.Entity<ShopProduct>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Review>()
                .Property(r => r.Rating)
                .HasPrecision(2, 1);
        }
    }

}

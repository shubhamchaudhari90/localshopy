using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ProductMaster> Categories { get; set; }
    }
}

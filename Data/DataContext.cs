using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_MVC.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        private readonly DbContextOptions _options;
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            _options = options;

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Specify SQL server column type for UnitPrice property
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");
        }
        public DbSet<Product> Products { get; set; }
    }
}

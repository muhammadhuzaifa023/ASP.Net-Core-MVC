using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_MVC.Data
{

   

    public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>

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


            // Customizing the ASP.NET Identity model and overriding the defaults if needed
            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasOne<ApplicationRole>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            //The following code will set ON DELETE NO ACTION to all Foreign Keys
            //foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            //{
            //    foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            //}
        }

        public DbSet<Product> Products { get; set; }
    }
}

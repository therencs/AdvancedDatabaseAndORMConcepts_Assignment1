using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class LaptopStoresContext : DbContext
    {
        public LaptopStoresContext(DbContextOptions options): base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Laptop>().HasKey(l => l.Number);

            modelBuilder.Entity<StoreLocation>().HasKey(s => s.StoreNumber);

            modelBuilder.Entity<StoreLaptop>()
                .HasKey(sl => new { sl.StoreId, sl.LaptopId });

            modelBuilder.Entity<StoreLaptop>()
                .HasOne(sl => sl.Laptop)
                .WithMany(l => l.StoreLaptops)
                .HasForeignKey(sl => sl.LaptopId);

            modelBuilder.Entity<StoreLaptop>()
                .HasOne(sl => sl.StoreLocation)
                .WithMany(l => l.StoreLaptops)
                .HasForeignKey(sl => sl.StoreId);

            modelBuilder.Entity<Laptop>(e =>
            {
                e.Property(ep => ep.Price)
                    .HasPrecision(20, 2);
            });

            modelBuilder.Entity<Laptop>().Ignore(l => l.StoreLaptops);
        }


        public DbSet<Laptop> Laptops { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<StoreLocation> StoreLocations { get; set; } = null!;
        public DbSet<StoreLaptop> StoreLaptops { get; set; } = null!;
    }
}

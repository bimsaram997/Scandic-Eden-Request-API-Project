using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemCategory> ItemCategories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial categories so the database isn't completely empty
            modelBuilder.Entity<ItemCategory>().HasData(
                new ItemCategory { Id = 1, Name = "Linen" },
                new ItemCategory { Id = 2, Name = "Glasses" },
                new ItemCategory { Id = 3, Name = "Amenities" }
            );
        }
    }
}

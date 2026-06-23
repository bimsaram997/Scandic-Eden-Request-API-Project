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
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<RequestHeader> RequestHeaders { get; set; } = null!;
        public DbSet<RequestLine> RequestLines { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial categories so the database isn't completely empty
            modelBuilder.Entity<ItemCategory>().HasData(
                new ItemCategory { Id = 1, Name = "Linen" },
                new ItemCategory { Id = 2, Name = "Glasses" },
                new ItemCategory { Id = 3, Name = "Amenities" }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, Name = "Mika (Cleaner)", Role = "Housekeeper" },
                new Employee { Id = 2, Name = "James (Cleaner)", Role = "Housekeeper" },
                new Employee { Id = 3, Name = "Laura (Leader)", Role = "TeamLeader" }
            );
            modelBuilder.Entity<RequestLine>()
                .HasOne(l => l.RequestHeader)
                .WithMany(h => h.Lines)
                .HasForeignKey(l => l.RequestHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

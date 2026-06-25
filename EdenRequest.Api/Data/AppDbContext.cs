using EdenRequest.Api.Data;
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
                new Employee { Id = 1, Name = "Mika (Cleaner)", Email = "mike@gmail.com", Password = "123", Role = "Housekeeper" },
                new Employee { Id = 2, Name = "James (Cleaner)", Email = "James@gmail.com", Password = "123", Role = "Housekeeper" },
                new Employee { Id = 3, Name = "Laura (Leader)", Email = "Laura@gmail.com", Password = "123", Role = "TeamLeader" },
                 new Employee { Id = 4, Name = "Grace (Leader)", Email = "Grace@gmail.com", Password = "123", Role = "TeamLeader" }
            );
            modelBuilder.Entity<RequestLine>()
                .HasOne(l => l.RequestHeader)
                .WithMany(h => h.Lines)
                .HasForeignKey(l => l.RequestHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


//new Employee { Id = 1, Name = "Mika (Cleaner)", Email = "mike@gmail.com", Password = "123", Role = "Housekeeper" },
//                new Employee { Id = 2, Name = "James (Cleaner)", Email = "James@gmail.com", Password = "123", Role = "Housekeeper" },
//                new Employee { Id = 3, Name = "Laura (Leader)", Email = "Laura@gmail.com", Password = "123", Role = "TeamLeader" }
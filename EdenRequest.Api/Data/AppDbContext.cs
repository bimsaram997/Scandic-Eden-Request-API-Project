using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemCategory> ItemCategories { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<RequestHeader> RequestHeaders { get; set; } = null!;
        public DbSet<RequestLine> RequestLines { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<ExtraWorkItem> ExtraWorkItems { get; set; } = null!;
        public DbSet<ExtraWorkRequest> ExtraWorkRequests { get; set; } = null!;
        public DbSet<ExtraRequestLine> ExtraRequestLines { get; set; } = null!;
        public DbSet<ExtraDirtyReport> ExtraDirtyReports { get; set; } = null!;
        public DbSet<MediaFile> MediaFiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial categories
            modelBuilder.Entity<ItemCategory>().HasData(
                new ItemCategory { Id = 1, Name = "Linen" },
                new ItemCategory { Id = 2, Name = "Glasses" },
                new ItemCategory { Id = 3, Name = "Amenities" }
            );

            // Seed employees
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

            // 💡 Explicit Foreign Key Mapping for ExtraDirtyReport -> Employee
            modelBuilder.Entity<ExtraDirtyReport>()
                .HasOne(r => r.ReportedBy)
                .WithMany()
                .HasForeignKey(r => r.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            // 💡 One-to-Many Relationship: ExtraDirtyReport -> MediaFiles
            modelBuilder.Entity<ExtraDirtyReport>()
                .HasMany(r => r.MediaFiles)
                .WithOne()
                .HasForeignKey(m => m.ExtraDirtyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
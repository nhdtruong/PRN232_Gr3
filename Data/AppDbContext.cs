using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Class> Classes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Class>().HasData(
                new Class { Id = 1, ClassName = "Class 10A1", CenterId = 1 },
                new Class { Id = 2, ClassName = "Class 10A2", CenterId = 1 },
                new Class { Id = 3, ClassName = "Class 11B1", CenterId = 2 },
                new Class { Id = 4, ClassName = "Class 11B2", CenterId = 2 },
                new Class { Id = 5, ClassName = "Class 12C1", CenterId = 3 }
            );
        }
    }
}

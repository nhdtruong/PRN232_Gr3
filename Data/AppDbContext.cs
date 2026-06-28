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
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique username for authentication
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<ClassStudent>()
                .HasKey(cs => new { cs.ClassId, cs.StudentId });

            modelBuilder.Entity<ClassStudent>()
                .HasOne(cs => cs.Class)
                .WithMany(c => c.ClassStudents)
                .HasForeignKey(cs => cs.ClassId);

            modelBuilder.Entity<ClassStudent>()
                .HasOne(cs => cs.Student)
                .WithMany(s => s.ClassStudents)
                .HasForeignKey(cs => cs.StudentId);

            // Prevent multiple cascade delete paths from User -> Class/Student/Notification
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Center)
                .WithMany(u => u.CreatedClasses)
                .HasForeignKey(c => c.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithMany(u => u.Students)
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Parent)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assessment>()
                .Property(a => a.Score)
                .HasColumnType("decimal(4,2)");

            modelBuilder.Entity<Assessment>()
                .HasIndex(a => new { a.StudentId, a.LessonId })
                .IsUnique();

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.LessonId })
                .IsUnique();

            modelBuilder.Entity<ChatChannel>()
                .HasIndex(cc => new { cc.CenterId, cc.ParentId })
                .IsUnique();

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.Center)
                .WithMany(u => u.CenterChannels)
                .HasForeignKey(cc => cc.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.Parent)
                .WithMany(u => u.ParentChannels)
                .HasForeignKey(cc => cc.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
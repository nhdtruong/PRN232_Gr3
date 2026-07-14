using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Data
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
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ClassTransferRequest> ClassTransferRequests { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Slot> Slots { get; set; }

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

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(u => u.TaughtClasses)
                .HasForeignKey(c => c.TeacherId)
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

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Room)
                .WithMany(r => r.Lessons)
                .HasForeignKey(l => l.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Slot)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Subject (Môn học) ----
            modelBuilder.Entity<Subject>()
                .HasOne(sub => sub.Center)
                .WithMany()
                .HasForeignKey(sub => sub.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassTransferRequest>()
                .HasOne(r => r.FromTeacher)
                .WithMany()
                .HasForeignKey(r => r.FromTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassTransferRequest>()
                .HasOne(r => r.ToTeacher)
                .WithMany()
                .HasForeignKey(r => r.ToTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassTransferRequest>()
                .HasOne(r => r.Class)
                .WithMany()
                .HasForeignKey(r => r.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mã môn học phải duy nhất trong phạm vi từng trung tâm
            modelBuilder.Entity<Subject>()
                .HasIndex(sub => new { sub.CenterId, sub.SubjectCode })
                .IsUnique();

            // Subject <-> Material (1 môn có nhiều tài liệu)
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Subject)
                .WithMany(sub => sub.Materials)
                .HasForeignKey(m => m.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Material -> Lesson (optional, nullable)
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Lesson)
                .WithMany(l => l.Materials)
                .HasForeignKey(m => m.LessonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
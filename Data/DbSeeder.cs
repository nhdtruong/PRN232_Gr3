using System.Linq;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Chỉ thêm nếu Database đang trống (chưa có User nào)
            if (!context.Users.Any())
            {
                var centerUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    FullName = "Chủ Trung Tâm",
                    Email = "admin@edubridge.com",
                    Phone = "0987654321",
                    Role = "Center",
                    IsActive = true,
                    CreatedAt = System.DateTime.Now
                };

                var parentUser = new User
                {
                    Username = "parent",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    FullName = "Nguyễn Văn Phụ Huynh",
                    Email = "parent@edubridge.com",
                    Phone = "0123456789",
                    Role = "Parent",
                    IsActive = true,
                    CreatedAt = System.DateTime.Now
                };

                context.Users.AddRange(centerUser, parentUser);
                context.SaveChanges();

                // Seed Classes
                var class1 = new Class { CenterId = centerUser.Id, ClassName = "Class 10A1", Status = "Active", CreatedAt = System.DateTime.Now };
                var class2 = new Class { CenterId = centerUser.Id, ClassName = "Class 10A2", Status = "Active", CreatedAt = System.DateTime.Now };
                var class3 = new Class { CenterId = centerUser.Id, ClassName = "Class 11B1", Status = "Closed", CreatedAt = System.DateTime.Now };
                context.Classes.AddRange(class1, class2, class3);
                context.SaveChanges();

                // Seed Students
                var student1 = new Student { ParentId = parentUser.Id, FullName = "Nguyễn Văn An", CreatedAt = System.DateTime.Now };
                var student2 = new Student { ParentId = parentUser.Id, FullName = "Nguyễn Thị Bình", CreatedAt = System.DateTime.Now };
                context.Students.AddRange(student1, student2);
                context.SaveChanges();

                // Seed ClassStudent Enrollments
                var enrollment1 = new ClassStudent { ClassId = class1.Id, StudentId = student1.Id, EnrolledAt = System.DateTime.Now };
                var enrollment2 = new ClassStudent { ClassId = class2.Id, StudentId = student2.Id, EnrolledAt = System.DateTime.Now };
                context.ClassStudents.AddRange(enrollment1, enrollment2);
                context.SaveChanges();
            }
        }
    }
}

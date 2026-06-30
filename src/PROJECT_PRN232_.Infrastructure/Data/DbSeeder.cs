using System.Linq;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // 1. Tạo Users nếu chưa có
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

            // 0. Seed Rooms & Slots
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(
                    new Room { RoomName = "Phòng 101", Capacity = 30, Equipment = "Máy chiếu, Điều hòa, Bảng tương tác", Status = "Active" },
                    new Room { RoomName = "Phòng Test 01", Capacity = 20, Equipment = "Máy chiếu, Điều hòa", Status = "Active" },
                    new Room { RoomName = "Phòng Test 02", Capacity = 25, Equipment = "Máy chiếu, Điều hòa, Tivi 75 inch", Status = "Active" },
                    new Room { RoomName = "Phòng Lab A", Capacity = 40, Equipment = "40 Máy tính cấu hình cao, Máy chiếu", Status = "Active" }
                );
                context.SaveChanges();
            }

            if (!context.Slots.Any())
            {
                context.Slots.AddRange(
                    new Slot { SlotName = "Ca 1 (Sáng)", StartTime = new System.TimeSpan(8, 0, 0), EndTime = new System.TimeSpan(9, 30, 0) },
                    new Slot { SlotName = "Ca 2 (Sáng)", StartTime = new System.TimeSpan(10, 0, 0), EndTime = new System.TimeSpan(11, 30, 0) },
                    new Slot { SlotName = "Ca 3 (Chiều)", StartTime = new System.TimeSpan(14, 0, 0), EndTime = new System.TimeSpan(15, 30, 0) },
                    new Slot { SlotName = "Ca 4 (Chiều)", StartTime = new System.TimeSpan(16, 0, 0), EndTime = new System.TimeSpan(17, 30, 0) },
                    new Slot { SlotName = "Ca Tối", StartTime = new System.TimeSpan(18, 0, 0), EndTime = new System.TimeSpan(19, 30, 0) }
                );
                context.SaveChanges();
            }

            // Lấy ID của Center và Parent hiện có để phục vụ liên kết
            var center = context.Users.FirstOrDefault(u => u.Role == "Center");
            var parent = context.Users.FirstOrDefault(u => u.Role == "Parent");

            if (center != null && parent != null)
            {
                // 2. Tạo Classes mẫu nếu chưa có
                if (!context.Classes.Any())
                {
                    var class1 = new Class
                    {
                        ClassName = "Lớp Lập trình C# Cơ bản - PRN232",
                        CenterId = center.Id,
                        Status = "Active",
                        CreatedAt = System.DateTime.Now
                    };

                    var class2 = new Class
                    {
                        ClassName = "Lớp Phát triển Web với ASP.NET Core - PRN211",
                        CenterId = center.Id,
                        Status = "Active",
                        CreatedAt = System.DateTime.Now
                    };

                    context.Classes.AddRange(class1, class2);
                    context.SaveChanges();
                }

                // 3. Tạo Students mẫu nếu chưa có
                if (!context.Students.Any())
                {
                    var student1 = new Student
                    {
                        FullName = "Nguyễn Văn A (Con Cưng)",
                        DateOfBirth = new System.DateTime(2015, 5, 10),
                        ParentId = parent.Id
                    };

                    var student2 = new Student
                    {
                        FullName = "Nguyễn Thị B (Con Thứ)",
                        DateOfBirth = new System.DateTime(2017, 8, 20),
                        ParentId = parent.Id
                    };

                    context.Students.AddRange(student1, student2);
                    context.SaveChanges();
                }

                // 4. Liên kết học sinh vào Lớp học (ClassStudent) nếu chưa có
                if (!context.ClassStudents.Any())
                {
                    var firstClass = context.Classes.FirstOrDefault();
                    var firstStudent = context.Students.FirstOrDefault();

                    if (firstClass != null && firstStudent != null)
                    {
                        var classStudent = new ClassStudent
                        {
                            ClassId = firstClass.Id,
                            StudentId = firstStudent.Id
                        };
                        context.ClassStudents.Add(classStudent);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}

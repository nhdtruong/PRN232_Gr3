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
            }
        }
    }
}

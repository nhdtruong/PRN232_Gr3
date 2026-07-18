using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        [RegularExpression(@"^(\+84|0)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ (10 chữ số, bắt đầu bằng 0 hoặc +84).")]
        public string? Phone { get; set; }
    }
}

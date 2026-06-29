using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vai trò không được để trống")]
        public string Role { get; set; } = "Parent"; // Default is Parent, or Center
    }
}

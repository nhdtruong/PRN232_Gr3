using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }
}

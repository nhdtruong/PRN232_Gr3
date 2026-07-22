using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class AuthService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IStudentRepository studentRepository, IConfiguration configuration)
        {
            _studentRepository = studentRepository;
            _configuration = configuration;
        }
        // 1.5. Logic Kiểm tra thông tin đăng nhập (Dùng chung cho cả API và Razor Pages)
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _studentRepository.FindUserForAuthAsync(username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        // 2. Logic Đăng nhập & Tạo Token JWT (Dành cho API)
        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await AuthenticateAsync(dto.Username, dto.Password);
            
            // Kiểm tra user tồn tại và đang hoạt động
            if (user == null || !user.IsActive)
                return null; 

            // Tạo danh sách Claims (Thông tin đính kèm trong thẻ token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), // Lưu Role để phân quyền
                new Claim("FullName", user.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // Token có hiệu lực trong 2 tiếng
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(createdToken); // Trả về chuỗi Token dạng String
        }

        // 3. Logic Đổi mật khẩu
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _studentRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _studentRepository.UpdateUserAsync(user);
            return true;
        }
    }
}

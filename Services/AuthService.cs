using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace PROJECT_PRN232_.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Logic Đăng ký tài khoản (Có băm mật khẩu)
        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return false; // Tài khoản đã tồn tại

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Băm mật khẩu ở đây
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // 1.5. Logic Kiểm tra thông tin đăng nhập (Dùng chung cho cả API và Razor Pages)
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == username || 
                u.Email == username || 
                u.Phone == username);

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
                new Claim(ClaimTypes.Role, user.Role) // Lưu Role để phân quyền
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
    }
}

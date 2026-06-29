using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);
            if (!result) return BadRequest(new { message = "Tên tài khoản đã tồn tại!" });
            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy thông tin user để set Cookie
            var user = await _authService.AuthenticateAsync(dto.Username, dto.Password);
            if (user == null || !user.IsActive)
            {
                return UniversalAuthError();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Set Cookie cho trình duyệt Web
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Lấy JWT cho Mobile App / API Client
            var token = await _authService.LoginAsync(dto);
            
            return Ok(new { token = token, role = user.Role });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công!" });
        }

        [HttpPost("change-password")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Mật khẩu cũ không chính xác hoặc lỗi hệ thống." });
            }

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        private IActionResult UniversalAuthError() => 
            Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
    }
}

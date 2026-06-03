using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;
using Microsoft.AspNetCore.Mvc;

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

            var token = await _authService.LoginAsync(dto);
            if (token == null) return UniversalAuthError();
            return Ok(new { token = token });
        }

        private IActionResult UniversalAuthError() => 
            Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
    }
}

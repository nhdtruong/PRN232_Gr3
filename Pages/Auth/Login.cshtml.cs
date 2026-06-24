using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJECT_PRN232_.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tài khoản hoặc email.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            public string Password { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard(User.FindFirst(ClaimTypes.Role)?.Value);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return Page();
            }

            // Gọi logic Auth từ Service cũ (Chấp nhận Username/Email/Phone)
            var user = await _authService.AuthenticateAsync(Input.Email, Input.Password);
            
            if (user == null)
            {
                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Ghi thẻ Cookie vào trình duyệt
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["ToastMessage"] = "Đăng nhập thành công!";
            return RedirectToDashboard(user.Role);
        }

        private IActionResult RedirectToDashboard(string? role)
        {
            if (role == "Parent")
                return RedirectToPage("/Parent/Dashboard");
            
            if (role == "Center")
                return RedirectToPage("/Center/Dashboard");
            
            // Dành cho Role khác
            return RedirectToPage("/Index");
        }
    }
}

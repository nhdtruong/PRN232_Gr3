using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PROJECT_PRN232_.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Kiểm tra nếu đã đăng nhập thì đẩy vào Dashboard tương ứng
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "Parent")
                    return RedirectToPage("/Parent/Dashboard");
                if (role == "Center")
                    return RedirectToPage("/Center/Dashboard");
                if (role == "Teacher")
                    return RedirectToPage("/Teacher/Dashboard");
            }
            
            // Nếu chưa đăng nhập, đá thẳng ra trang Login luôn
            return RedirectToPage("/Auth/Login");
        }
    }
}

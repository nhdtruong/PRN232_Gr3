using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.WebApp.Pages.Parent
{
    // Bắt buộc phải đăng nhập và có Role là Parent mới được vào
    [Authorize(Roles = "Parent")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // Nơi này sau này sẽ gọi API lấy thông tin Profile Phụ huynh
        }
    }
}

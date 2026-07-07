using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

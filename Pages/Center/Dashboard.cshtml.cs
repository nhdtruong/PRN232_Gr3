using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.Pages.Center
{
    [Authorize(Roles = "Center")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

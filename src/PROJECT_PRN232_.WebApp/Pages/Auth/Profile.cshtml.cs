using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace PROJECT_PRN232_.WebApp.Pages.Auth
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

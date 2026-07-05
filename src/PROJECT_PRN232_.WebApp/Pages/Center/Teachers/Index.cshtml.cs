using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Teachers
{
    [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

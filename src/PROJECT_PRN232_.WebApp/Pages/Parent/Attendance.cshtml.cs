using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Parent
{
    [Authorize(Roles = "Parent")]
    public class AttendanceModel : PageModel
    {
        public AttendanceModel()
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
    }
}

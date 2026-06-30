using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Subjects
{
    [Authorize(Roles = "Center")]
    public class SubjectModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}

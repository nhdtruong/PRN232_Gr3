using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class StudentsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        public void OnGet()
        {
        }
    }
}

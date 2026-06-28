using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Lessons
{
    [Authorize(Roles = "Center")]
    public class LessonModel : PageModel
    {
        public LessonModel()
        {
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (ClassId <= 0)
            {
                return RedirectToPage("/Center/Dashboard");
            }
            return Page();
        }
    }
}

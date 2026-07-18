using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class MaterialsModel : PageModel
    {
        private readonly AppDbContext _context;

        public MaterialsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int LessonId { get; set; }

        public int ClassId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (LessonId <= 0)
            {
                return RedirectToPage("/Teacher/Dashboard");
            }

            // Bảo mật và nạp dữ liệu thực tế từ Database
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int teacherId))
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Class)
                    .FirstOrDefaultAsync(l => l.Id == LessonId && l.Class.TeacherId == teacherId);

                if (lesson == null)
                {
                    return RedirectToPage("/Teacher/Dashboard");
                }

                LessonTitle = lesson.Title;
                ClassName = lesson.Class.ClassName;
                ClassId = lesson.ClassId;
            }
            else
            {
                return RedirectToPage("/Teacher/Dashboard");
            }

            return Page();
        }
    }
}

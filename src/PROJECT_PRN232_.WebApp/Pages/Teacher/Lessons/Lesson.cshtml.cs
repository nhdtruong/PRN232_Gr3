using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class LessonModel : PageModel
    {
        private readonly AppDbContext _context;

        public LessonModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        public List<Room> Rooms { get; set; } = new();
        public List<Slot> Slots { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (ClassId <= 0)
            {
                return RedirectToPage("/Teacher/Dashboard");
            }

            // Bảo mật: Đảm bảo Giáo viên này sở hữu lớp học
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int teacherId))
            {
                var isOwner = await _context.Classes.AnyAsync(c => c.Id == ClassId && c.TeacherId == teacherId);
                if (!isOwner)
                {
                    return RedirectToPage("/Teacher/Dashboard");
                }
            }
            else
            {
                return RedirectToPage("/Teacher/Dashboard");
            }

            Rooms = await _context.Rooms.Where(r => r.Status == "Active").ToListAsync();
            Slots = await _context.Slots.ToListAsync();
            return Page();
        }
    }
}

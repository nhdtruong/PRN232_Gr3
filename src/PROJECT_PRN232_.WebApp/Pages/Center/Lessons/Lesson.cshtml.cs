using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Lessons
{
    [Authorize(Roles = "Center")]
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
                return RedirectToPage("/Center/Dashboard");
            }

            Rooms = await _context.Rooms.Where(r => r.Status == "Active").ToListAsync();
            Slots = await _context.Slots.ToListAsync();
            return Page();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Room> Rooms { get; set; } = new();
        public List<Slot> Slots { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<User> Teachers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms.OrderBy(r => r.RoomName).ToListAsync();
            Slots = await _context.Slots.OrderBy(s => s.StartTime).ToListAsync();
            Subjects = await _context.Subjects.OrderBy(s => s.SubjectName).ToListAsync();
            Teachers = await _context.Users.Where(u => u.Role == "Teacher").OrderBy(u => u.FullName).ToListAsync();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher
{
    [Authorize(Roles = "Teacher")]
    public class MyClassesModel : PageModel
    {
        private readonly AppDbContext _context;

        public MyClassesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Class> MyClasses { get; set; } = new();
        public List<User> OtherTeachers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int teacherId))
            {
                MyClasses = await _context.Classes
                    .Where(c => c.TeacherId == teacherId && c.Status == "Active")
                    .ToListAsync();

                OtherTeachers = await _context.Users
                    .Where(u => u.Role == "Teacher" && u.Id != teacherId)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }
        }
    }
}

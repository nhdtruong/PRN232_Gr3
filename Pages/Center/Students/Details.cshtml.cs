using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Students
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Student Student { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Parent)
                .Include(s => s.ClassStudents)
                    .ThenInclude(cs => cs.Class)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            return Page();
        }
    }
}

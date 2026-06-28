using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Parents
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public User ParentUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parent = await _context.Users
                .Include(u => u.Students)
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == "Parent");

            if (parent == null)
            {
                return NotFound();
            }
            
            ParentUser = parent;
            return Page();
        }
    }
}

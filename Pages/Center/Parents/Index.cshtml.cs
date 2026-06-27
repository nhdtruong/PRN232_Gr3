using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Parents
{
    // [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<User> ParentUsers { get; set; } = default!;

        public async Task OnGetAsync()
        {
            ParentUsers = await _context.Users
                .Where(u => u.Role == "Parent")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Parent")
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = user.IsActive ? "Đã mở khóa tài khoản thành công." : "Đã khóa tài khoản thành công.";
            return RedirectToPage("./Index");
        }
    }
}

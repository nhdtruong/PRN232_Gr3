using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Students
{
    [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<User> ParentUsers { get; set; } = new();

        public async Task OnGetAsync()
        {
            ParentUsers = await _context.Users
                .Where(u => u.Role == "Parent" && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
    }
}

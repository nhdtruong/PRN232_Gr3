using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.WebApp.Pages.Parent
{
    [Authorize(Roles = "Parent")]
    public class ChildrenClassesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IEnrollmentService _enrollmentService;

        public ChildrenClassesModel(AppDbContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        public IEnumerable<Student> Children { get; set; } = new List<Student>();
        public IEnumerable<Class> EnrolledClasses { get; set; } = new List<Class>();

        [BindProperty(SupportsGet = true)]
        public int? SelectedStudentId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Challenge();
            }

            var parentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (parentUser == null)
            {
                return NotFound();
            }

            // Get parent's children
            Children = await _context.Students.Where(s => s.ParentId == parentUser.Id).ToListAsync();

            if (Children.Any())
            {
                if (!SelectedStudentId.HasValue || !Children.Any(c => c.Id == SelectedStudentId.Value))
                {
                    SelectedStudentId = Children.First().Id;
                }

                EnrolledClasses = await _enrollmentService.GetClassesForStudentAsync(SelectedStudentId.Value, parentUser.Id);
            }

            return Page();
        }
    }
}

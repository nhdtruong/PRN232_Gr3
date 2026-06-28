using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Students
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public SelectList ParentList { get; set; } = default!;

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn phụ huynh")]
            [Display(Name = "Phụ huynh")]
            public int ParentId { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [Display(Name = "Họ và Tên")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Giới tính")]
            public string? Gender { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = student.Id,
                ParentId = student.ParentId,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender
            };

            await LoadParentsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadParentsAsync();
                return Page();
            }

            var studentToUpdate = await _context.Students.FindAsync(Input.Id);
            if (studentToUpdate == null)
            {
                return NotFound();
            }

            studentToUpdate.ParentId = Input.ParentId;
            studentToUpdate.FullName = Input.FullName;
            studentToUpdate.DateOfBirth = Input.DateOfBirth;
            studentToUpdate.Gender = Input.Gender;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin học sinh thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(studentToUpdate.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadParentsAsync()
        {
            var parents = await _context.Users
                .Where(u => u.Role == "Parent")
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ParentList = new SelectList(parents, "Id", "FullName");
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}

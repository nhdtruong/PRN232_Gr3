using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Infrastructure.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Students
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public SelectList ParentList { get; set; } = default!;

        public class InputModel
        {
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

        public async Task OnGetAsync()
        {
            var parents = await _context.Users
                .Where(u => u.Role == "Parent")
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ParentList = new SelectList(parents, "Id", "FullName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var parents = await _context.Users
                    .Where(u => u.Role == "Parent")
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
                ParentList = new SelectList(parents, "Id", "FullName");
                return Page();
            }

            var student = new Student
            {
                ParentId = Input.ParentId,
                FullName = Input.FullName,
                DateOfBirth = Input.DateOfBirth,
                Gender = Input.Gender,
                CreatedAt = DateTime.Now
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm học sinh thành công.";
            return RedirectToPage("./Index");
        }
    }
}

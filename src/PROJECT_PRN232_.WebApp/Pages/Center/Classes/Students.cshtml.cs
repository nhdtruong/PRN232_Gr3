using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class StudentsModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IClassService _classService;
        private readonly AppDbContext _context;

        public StudentsModel(IEnrollmentService enrollmentService, IClassService classService, AppDbContext context)
        {
            _enrollmentService = enrollmentService;
            _classService = classService;
            _context = context;
        }

        public ClassResponseDto ClassInfo { get; set; } = null!;
        public IEnumerable<Student> EnrolledStudents { get; set; } = new List<Student>();
        public IEnumerable<Student> AvailableStudents { get; set; } = new List<Student>();
        public IEnumerable<ClassResponseDto> ActiveClasses { get; set; } = new List<ClassResponseDto>();

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        [BindProperty]
        public List<int> SelectedStudentIds { get; set; } = new();

        [BindProperty]
        public int StudentId { get; set; }

        [BindProperty]
        public int TargetClassId { get; set; }

        public async Task<IActionResult> OnGetAsync(int classId)
        {
            ClassId = classId;
            var classObj = await _classService.GetClassByIdAsync(classId);
            if (classObj == null)
            {
                return NotFound();
            }
            ClassInfo = classObj;

            // Load enrolled students
            var enrolledList = await _enrollmentService.GetStudentsInClassAsync(classId);
            EnrolledStudents = enrolledList.ToList();
            var enrolledIds = EnrolledStudents.Select(s => s.Id).ToList();

            // Load available students (not in this class)
            AvailableStudents = await _context.Students
                .Where(s => !enrolledIds.Contains(s.Id))
                .OrderBy(s => s.FullName)
                .ToListAsync();

            // Load other active classes for transfer options
            var allClasses = await _classService.GetAllClassesAsync();
            ActiveClasses = allClasses.Where(c => c.Status == "Active" && c.Id != classId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostEnrollMultiAsync()
        {
            if (SelectedStudentIds == null || !SelectedStudentIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một học sinh từ danh sách để xếp lớp!";
                return RedirectToPage(new { classId = ClassId });
            }

            int successCount = 0;
            int failCount = 0;
            List<string> errorMessages = new();

            foreach (var studentId in SelectedStudentIds)
            {
                try
                {
                    var success = await _enrollmentService.EnrollStudentAsync(ClassId, studentId);
                    if (success) successCount++;
                    else failCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    errorMessages.Add(ex.Message);
                }
            }

            if (successCount > 0)
            {
                TempData["SuccessMessage"] = $"Đã xếp lớp thành công cho {successCount} học sinh!";
            }
            if (failCount > 0)
            {
                TempData["ErrorMessage"] = $"Có {failCount} học sinh không thể xếp lớp. Chi tiết: " + string.Join(", ", errorMessages.Distinct());
            }

            return RedirectToPage(new { classId = ClassId });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int studentId)
        {
            var success = await _enrollmentService.RemoveStudentFromClassAsync(ClassId, studentId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa học viên ra khỏi lớp thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa học viên.";
            }

            return RedirectToPage(new { classId = ClassId });
        }

        public async Task<IActionResult> OnPostTransferAsync(int studentId)
        {
            try
            {
                await _enrollmentService.TransferStudentClassAsync(studentId, ClassId, TargetClassId);
                TempData["SuccessMessage"] = "Chuyển lớp cho học viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { classId = ClassId });
        }
    }
}

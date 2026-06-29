using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class StudentsModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IClassService _classService;

        public StudentsModel(IEnrollmentService enrollmentService, IClassService classService)
        {
            _enrollmentService = enrollmentService;
            _classService = classService;
        }

        public ClassResponseDto ClassInfo { get; set; } = null!;
        public IEnumerable<Student> EnrolledStudents { get; set; } = new List<Student>();
        public IEnumerable<ClassResponseDto> ActiveClasses { get; set; } = new List<ClassResponseDto>();

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

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
            EnrolledStudents = await _enrollmentService.GetStudentsInClassAsync(classId);

            var allClasses = await _classService.GetAllClassesAsync();
            ActiveClasses = allClasses.Where(c => c.Status == "Active" && c.Id != classId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostEnrollAsync()
        {
            if (StudentId <= 0)
            {
                TempData["ErrorMessage"] = "Mã học sinh phải là số dương lớn hơn 0.";
                return RedirectToPage(new { classId = ClassId });
            }

            try
            {
                await _enrollmentService.EnrollStudentAsync(ClassId, StudentId);
                TempData["SuccessMessage"] = "Thêm học sinh vào lớp học thành công!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { classId = ClassId });
        }
    }
}

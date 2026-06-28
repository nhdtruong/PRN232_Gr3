using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        private readonly IClassService _classService;

        public IndexModel(IClassService classService)
        {
            _classService = classService;
        }

        public IEnumerable<ClassResponseDto> Classes { get; set; } = new List<ClassResponseDto>();

        [BindProperty]
        public ClassCreateDto CreateDto { get; set; } = new();

        [BindProperty]
        public ClassUpdateDto UpdateDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            Classes = await _classService.GetAllClassesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Reset ModelState validation state to only validate CreateDto
            ModelState.Clear();
            if (!TryValidateModel(CreateDto, nameof(CreateDto)))
            {
                Classes = await _classService.GetAllClassesAsync();
                return Page();
            }

            await _classService.CreateClassAsync(CreateDto);
            TempData["SuccessMessage"] = "Tạo lớp học thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            // Reset ModelState validation state to only validate UpdateDto
            ModelState.Clear();
            if (!TryValidateModel(UpdateDto, nameof(UpdateDto)))
            {
                Classes = await _classService.GetAllClassesAsync();
                return Page();
            }

            var success = await _classService.UpdateClassAsync(UpdateDto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy lớp học để sửa.");
                Classes = await _classService.GetAllClassesAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin lớp thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, string currentStatus)
        {
            var newStatus = currentStatus == "Active" ? "Closed" : "Active";
            var success = await _classService.UpdateClassStatusAsync(id, newStatus);
            
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã {(newStatus == "Closed" ? "đóng" : "mở")} lớp học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể thay đổi trạng thái lớp.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _classService.DeleteClassAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa lớp học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học để xóa.";
            }
            return RedirectToPage();
        }
    }
}

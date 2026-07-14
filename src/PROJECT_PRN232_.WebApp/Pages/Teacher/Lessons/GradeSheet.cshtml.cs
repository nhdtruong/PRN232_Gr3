using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class GradeSheetModel : PageModel
    {
        private readonly ILessonRollCallService _rollCallService;
        private readonly AppDbContext _context;

        public GradeSheetModel(ILessonRollCallService rollCallService, AppDbContext context)
        {
            _rollCallService = rollCallService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int LessonId { get; set; }

        public LessonRollCallResponseDto? RollCallData { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LessonId <= 0)
            {
                ErrorMessage = "Mã buổi học không hợp lệ. Vui lòng kiểm tra lại đường dẫn.";
                return Page();
            }

            // Bảo mật: Đảm bảo Giáo viên này sở hữu lớp học chứa buổi học
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int teacherId))
            {
                var ownsLesson = await _context.Lessons
                    .Include(l => l.Class)
                    .AnyAsync(l => l.Id == LessonId && l.Class.TeacherId == teacherId);

                if (!ownsLesson)
                {
                    ErrorMessage = "Bạn không có quyền truy cập kết quả buổi học này.";
                    return Page();
                }
            }
            else
            {
                ErrorMessage = "Không thể xác định thông tin đăng nhập.";
                return Page();
            }

            RollCallData = await _rollCallService.GetRollCallByLessonAsync(LessonId);

            if (RollCallData == null)
            {
                ErrorMessage = $"Không tìm thấy thông tin buổi học với mã #{LessonId}.";
            }

            return Page();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Lessons
{
    [Authorize(Roles = "Center")]
    public class GradeSheetModel : PageModel
    {
        private readonly ILessonRollCallService _rollCallService;

        public GradeSheetModel(ILessonRollCallService rollCallService)
        {
            _rollCallService = rollCallService;
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

            RollCallData = await _rollCallService.GetRollCallByLessonAsync(LessonId);

            if (RollCallData == null)
            {
                ErrorMessage = $"Không tìm thấy thông tin buổi học với mã #{LessonId}.";
            }

            return Page();
        }
    }
}

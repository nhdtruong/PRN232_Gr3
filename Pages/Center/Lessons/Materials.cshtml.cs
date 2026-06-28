using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Lessons
{
    [Authorize(Roles = "Center")]
    public class MaterialsModel : PageModel
    {
        public MaterialsModel()
        {
        }

        [BindProperty(SupportsGet = true)]
        public int LessonId { get; set; }

        public int ClassId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (LessonId <= 0)
            {
                return RedirectToPage("/Center/Dashboard");
            }

            // Gán dữ liệu hiển thị tạm thời cho Header, sau này kết nối DB Service lấy thông tin thực tế
            LessonTitle = "Buổi học " + LessonId;
            ClassName = "Lớp học mẫu";
            ClassId = 1; // Giả lập ClassId để quay lại danh sách buổi học

            return Page();
        }
    }
}

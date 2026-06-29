using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/lessons/{lessonId}/assessment")]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var parentFilter = GetParentFilter();
            var result = await _assessmentService.GetByLessonIdAsync(lessonId, parentFilter);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> SaveBulk(int lessonId, [FromBody] LessonAssessmentBulkDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _assessmentService.SaveBulkAsync(lessonId, dto, centerId);
            if (!success)
                return BadRequest(new { message = "Không thể lưu điểm số. Kiểm tra quyền, thang điểm (0-10) hoặc dữ liệu học sinh." });

            return NoContent();
        }

        private int? GetParentFilter()
        {
            if (User.IsInRole("Parent"))
                return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return null;
        }

        /// <summary>
        /// Phụ huynh xem lịch sử đánh giá nhận xét của con mình
        /// GET /api/parent/children/{studentId}/assessment
        /// </summary>
        [HttpGet("/api/parent/children/{studentId}/assessment")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetForParent(int studentId)
        {
            var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _assessmentService.GetByStudentIdAsync(studentId, parentId);
            return Ok(result);
        }
    }
}

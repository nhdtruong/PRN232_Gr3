using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/lessons/{lessonId}/attendance")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var parentFilter = GetParentFilter();
            var result = await _attendanceService.GetByLessonIdAsync(lessonId, parentFilter);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> SaveBulk(int lessonId, [FromBody] LessonAttendanceBulkDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _attendanceService.SaveBulkAsync(lessonId, dto, centerId);
            if (!success)
                return BadRequest(new { message = "Không thể lưu điểm danh. Kiểm tra quyền hoặc dữ liệu học sinh." });

            return NoContent();
        }

        private int? GetParentFilter()
        {
            if (User.IsInRole("Parent"))
                return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return null;
        }
    }
}

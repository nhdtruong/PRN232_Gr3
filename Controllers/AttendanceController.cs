using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Lấy danh sách điểm danh theo buổi học
        /// GET /api/lessons/{lessonId}/attendance
        /// </summary>
        [HttpGet("api/lessons/{lessonId}/attendance")]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var parentFilter = GetParentFilter();
            var result = await _attendanceService.GetByLessonIdAsync(lessonId, parentFilter);
            return Ok(result);
        }

        /// <summary>
        /// Lưu điểm danh hàng loạt (Bulk) cho buổi học
        /// POST /api/lessons/{lessonId}/attendance
        /// </summary>
        [HttpPost("api/lessons/{lessonId}/attendance")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> SaveBulk(int lessonId, [FromBody] LessonAttendanceBulkDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = GetUserId();
            var success = await _attendanceService.SaveBulkAsync(lessonId, dto, centerId);
            if (!success)
                return BadRequest(new { message = "Không thể lưu điểm danh. Kiểm tra quyền hoặc dữ liệu học sinh." });

            return NoContent();
        }

        /// <summary>
        /// Sửa 1 bản ghi điểm danh đơn lẻ (Center sửa)
        /// PUT /api/center/attendance/{attendanceId}
        /// </summary>
        [HttpPut("api/center/attendance/{attendanceId}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> UpdateSingle(int attendanceId, [FromBody] AttendanceUpsertDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = GetUserId();
            var success = await _attendanceService.UpdateSingleAsync(attendanceId, dto, centerId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật điểm danh. Vui lòng kiểm tra quyền sở hữu hoặc ID điểm danh." });
            }

            return NoContent();
        }

        /// <summary>
        /// Phụ huynh xem lịch sử điểm danh của con mình
        /// GET /api/parent/children/{studentId}/attendance
        /// </summary>
        [HttpGet("api/parent/children/{studentId}/attendance")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetForParent(int studentId)
        {
            var parentId = GetUserId();
            var result = await _attendanceService.GetByStudentIdAsync(studentId, parentId);
            return Ok(result);
        }

        // Helper lấy User ID từ Claims
        private int GetUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idString, out int userId) ? userId : 0;
        }

        private int? GetParentFilter()
        {
            if (User.IsInRole("Parent"))
                return GetUserId();
            return null;
        }
    }
}

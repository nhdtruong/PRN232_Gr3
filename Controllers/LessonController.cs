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
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        // ======================= CENTER APIs =======================

        /// <summary>
        /// Lấy toàn bộ danh sách buổi học của một Lớp học (Center quản lý)
        /// GET /api/center/classes/{classId}/lessons
        /// </summary>
        [HttpGet("api/center/classes/{classId}/lessons")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> GetByClass(int classId)
        {
            var lessons = await _lessonService.GetByClassIdAsync(classId);
            return Ok(lessons);
        }

        /// <summary>
        /// Tạo buổi học mới cho Lớp học
        /// POST /api/center/classes/{classId}/lessons
        /// </summary>
        [HttpPost("api/center/classes/{classId}/lessons")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> Create(int classId, [FromBody] LessonCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.ClassId = classId;
            var centerUserId = GetUserId();

            var result = await _lessonService.CreateAsync(dto, centerUserId);
            if (result == null)
            {
                return BadRequest(new { message = "Không thể tạo buổi học. Vui lòng kiểm tra quyền sở hữu lớp học hoặc lớp học đã bị khóa/đóng." });
            }

            return CreatedAtAction(nameof(GetById), new { lessonId = result.Id }, result);
        }

        /// <summary>
        /// Xem chi tiết một buổi học
        /// GET /api/center/lessons/{lessonId}
        /// </summary>
        [HttpGet("api/center/lessons/{lessonId}", Name = "GetById")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> GetById(int lessonId)
        {
            var lesson = await _lessonService.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound(new { message = $"Không tìm thấy buổi học với mã #{lessonId}." });
            }
            return Ok(lesson);
        }

        /// <summary>
        /// Cập nhật thông tin buổi học
        /// PUT /api/center/lessons/{lessonId}
        /// </summary>
        [HttpPut("api/center/lessons/{lessonId}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> Update(int lessonId, [FromBody] LessonUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerUserId = GetUserId();
            var success = await _lessonService.UpdateAsync(lessonId, dto, centerUserId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật buổi học. Vui lòng kiểm tra quyền sở hữu hoặc trạng thái lớp học." });
            }

            return NoContent();
        }

        /// <summary>
        /// Xóa / Hủy buổi học
        /// DELETE /api/center/lessons/{lessonId}
        /// </summary>
        [HttpDelete("api/center/lessons/{lessonId}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> Delete(int lessonId)
        {
            var centerUserId = GetUserId();
            var success = await _lessonService.DeleteAsync(lessonId, centerUserId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể xóa buổi học. Vui lòng kiểm tra quyền sở hữu lớp học." });
            }

            return NoContent();
        }

        // ======================= PARENT APIs =======================

        /// <summary>
        /// Xem lịch học các buổi của con
        /// GET /api/parent/children/{studentId}/lessons
        /// </summary>
        [HttpGet("api/parent/children/{studentId}/lessons")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetLessonsForChild(int studentId)
        {
            var parentUserId = GetUserId();
            var lessons = await _lessonService.GetByStudentForParentAsync(studentId, parentUserId);
            return Ok(lessons);
        }

        // Helper lấy User ID đăng nhập từ Claims
        private int GetUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idString, out int userId) ? userId : 0;
        }
    }
}

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
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>
        /// Lấy danh sách học liệu của 1 buổi học
        /// GET /api/center/lessons/{lessonId}/materials
        /// </summary>
        [HttpGet("api/center/lessons/{lessonId}/materials")]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var list = await _materialService.GetByLessonIdAsync(lessonId);
            return Ok(list);
        }

        /// <summary>
        /// Upload tài liệu mới vào buổi học
        /// POST /api/center/lessons/{lessonId}/materials
        /// </summary>
        [HttpPost("api/center/lessons/{lessonId}/materials")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> Upload(int lessonId, [FromBody] MaterialCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerUserId = GetUserId();
            var result = await _materialService.CreateAsync(lessonId, dto, centerUserId);

            if (result == null)
            {
                return BadRequest(new { message = "Không thể tải lên tài liệu. Vui lòng kiểm tra quyền sở hữu buổi học." });
            }

            return Created($"/api/center/lessons/{lessonId}/materials", result);
        }

        /// <summary>
        /// Xóa tài liệu học tập
        /// DELETE /api/center/materials/{materialId}
        /// </summary>
        [HttpDelete("api/center/materials/{materialId}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> Delete(int materialId)
        {
            var centerUserId = GetUserId();
            var success = await _materialService.DeleteAsync(materialId, centerUserId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể xóa tài liệu. Tài liệu không tồn tại hoặc bạn không có quyền." });
            }

            return NoContent();
        }

        // Helper lấy User ID từ Claims
        private int GetUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idString, out int userId) ? userId : 0;
        }
    }
}

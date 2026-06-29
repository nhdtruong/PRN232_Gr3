using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/lessons/{lessonId}/rollcall")]
    [Authorize(Roles = "Center")]
    public class LessonRollCallController : ControllerBase
    {
        private readonly ILessonRollCallService _rollCallService;

        public LessonRollCallController(ILessonRollCallService rollCallService)
        {
            _rollCallService = rollCallService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int lessonId)
        {
            var result = await _rollCallService.GetRollCallByLessonAsync(lessonId);
            if (result == null)
                return NotFound(new { message = $"Lesson with ID {lessonId} not found" });

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Save(int lessonId, [FromBody] LessonRollCallBulkUpsertDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = GetUserId();
            var success = await _rollCallService.SaveRollCallAsync(lessonId, dto, centerId);
            if (!success)
                return BadRequest(new { message = "Không thể lưu điểm danh/điểm số. Kiểm tra quyền hoặc dữ liệu học sinh." });

            return NoContent();
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

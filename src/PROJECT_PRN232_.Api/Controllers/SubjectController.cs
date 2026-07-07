using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize(Roles = "Center")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ môn học của trung tâm đang đăng nhập
        /// GET /api/center/subjects
        /// </summary>
        [HttpGet("api/center/subjects")]
        public async Task<IActionResult> GetAll()
        {
            var centerId = GetUserId();
            var subjects = await _subjectService.GetByCenterIdAsync(centerId);
            return Ok(subjects);
        }

        /// <summary>
        /// Xem chi tiết một môn học
        /// GET /api/center/subjects/{subjectId}
        /// </summary>
        [HttpGet("api/center/subjects/{subjectId}", Name = "GetSubjectById")]
        public async Task<IActionResult> GetById(int subjectId)
        {
            var centerId = GetUserId();
            var subject = await _subjectService.GetByIdAsync(subjectId, centerId);

            if (subject == null)
                return NotFound(new { message = $"Không tìm thấy môn học #{subjectId} hoặc bạn không có quyền truy cập." });

            return Ok(subject);
        }

        /// <summary>
        /// Tạo mới môn học
        /// POST /api/center/subjects
        /// </summary>
        [HttpPost("api/center/subjects")]
        public async Task<IActionResult> Create([FromBody] SubjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = GetUserId();

            try
            {
                var result = await _subjectService.CreateAsync(dto, centerId);
                return CreatedAtAction(nameof(GetById), new { subjectId = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin môn học
        /// PUT /api/center/subjects/{subjectId}
        /// </summary>
        [HttpPut("api/center/subjects/{subjectId}")]
        public async Task<IActionResult> Update(int subjectId, [FromBody] SubjectUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var centerId = GetUserId();
            dto.Id = subjectId;

            try
            {
                var success = await _subjectService.UpdateAsync(dto, centerId);
                if (!success)
                    return NotFound(new { message = "Môn học không tồn tại hoặc bạn không có quyền chỉnh sửa." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa môn học (và toàn bộ tài liệu đính kèm)
        /// DELETE /api/center/subjects/{subjectId}
        /// </summary>
        [HttpDelete("api/center/subjects/{subjectId}")]
        public async Task<IActionResult> Delete(int subjectId)
        {
            var centerId = GetUserId();
            var success = await _subjectService.DeleteAsync(subjectId, centerId);

            if (!success)
                return NotFound(new { message = "Môn học không tồn tại hoặc bạn không có quyền xóa." });

            return NoContent();
        }

        // Helper lấy UserId từ Claims
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out int id) ? id : 0;
        }
    }
}

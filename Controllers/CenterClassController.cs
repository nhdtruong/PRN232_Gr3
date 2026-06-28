using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize(Roles = "Center")]
    public class CenterClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IEnrollmentService _enrollmentService;

        public CenterClassController(IClassService classService, IEnrollmentService enrollmentService)
        {
            _classService = classService;
            _enrollmentService = enrollmentService;
        }

        [HttpGet("api/center/classes")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _classService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("api/center/classes/{classId}")]
        public async Task<IActionResult> GetClassById(int classId)
        {
            var classObj = await _classService.GetClassByIdAsync(classId);
            if (classObj == null)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId}." });
            }
            return Ok(classObj);
        }

        [HttpPost("api/center/classes")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _classService.CreateClassAsync(dto);
            return CreatedAtAction(nameof(GetClassById), new { classId = created.Id }, created);
        }

        [HttpPut("api/center/classes/{classId}")]
        public async Task<IActionResult> UpdateClass(int classId, [FromBody] ClassUpdateDto dto)
        {
            if (classId != dto.Id)
            {
                return BadRequest(new { message = "Mã lớp học trong URL và Body không khớp." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _classService.UpdateClassAsync(dto);
            if (!updated)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId} để cập nhật." });
            }

            return NoContent();
        }

        [HttpPatch("api/center/classes/{classId}/status")]
        public async Task<IActionResult> PatchClassStatus(int classId, [FromBody] StatusPatchDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest(new { message = "Trạng thái không được để trống." });
            }

            var success = await _classService.UpdateClassStatusAsync(classId, dto.Status);
            if (!success)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId}." });
            }

            return NoContent();
        }

        [HttpGet("api/center/classes/{classId}/students")]
        public async Task<IActionResult> GetStudentsInClass(int classId)
        {
            var students = await _enrollmentService.GetStudentsInClassAsync(classId);
            return Ok(students);
        }

        [HttpPost("api/center/classes/{classId}/students")]
        public async Task<IActionResult> EnrollStudent(int classId, [FromBody] EnrollStudentDto dto)
        {
            try
            {
                await _enrollmentService.EnrollStudentAsync(classId, dto.StudentId);
                return Ok(new { message = "Đã xếp học sinh vào lớp thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("api/center/classes/{classId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudent(int classId, int studentId)
        {
            var result = await _enrollmentService.RemoveStudentFromClassAsync(classId, studentId);
            if (!result)
            {
                return NotFound(new { message = "Học sinh không tồn tại trong lớp này." });
            }
            return Ok(new { message = "Đã xóa học sinh khỏi lớp thành công." });
        }

        [HttpPost("api/center/students/{studentId}/transfer-class")]
        public async Task<IActionResult> TransferStudentClass(int studentId, [FromBody] TransferClassDto dto)
        {
            try
            {
                await _enrollmentService.TransferStudentClassAsync(studentId, dto.FromClassId, dto.ToClassId);
                return Ok(new { message = "Chuyển lớp thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class StatusPatchDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class EnrollStudentDto
    {
        public int StudentId { get; set; }
    }

    public class TransferClassDto
    {
        public int FromClassId { get; set; }
        public int ToClassId { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly AppDbContext _context;

        public ClassController(IClassService classService, AppDbContext context)
        {
            _classService = classService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            var classes = await _classService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var classDto = await _classService.GetClassByIdAsync(id);
            if (classDto == null)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }
            return Ok(classDto);
        }

        [HttpPost]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _classService.CreateClassAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID in URL and body must match" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _classService.UpdateClassAsync(dto);
            if (!success)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var success = await _classService.DeleteClassAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }

            return NoContent();
        }

        [HttpGet("my-classes")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyClasses()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int teacherId))
            {
                return Unauthorized(new { message = "Không xác định danh tính giáo viên." });
            }

            var classes = await _context.Classes
                .Where(c => c.TeacherId == teacherId && c.Status == "Active")
                .Select(c => new
                {
                    c.Id,
                    c.ClassName,
                    Subject = c.Subject ?? "Chưa có môn",
                    c.MaxCapacity,
                    c.TotalLessons
                })
                .ToListAsync();

            return Ok(classes);
        }

        [HttpGet("other-teachers")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetOtherTeachers()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int teacherId))
            {
                return Unauthorized(new { message = "Không xác định danh tính giáo viên." });
            }

            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher" && u.IsActive && u.Id != teacherId)
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username
                })
                .ToListAsync();

            return Ok(teachers);
        }
    }
}

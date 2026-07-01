using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize(Roles = "Center")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/center/students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students
                .Include(s => s.Parent)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender,
                    s.ParentId,
                    ParentName = s.Parent != null ? s.Parent.FullName : "Chưa có",
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("api/center/students/{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender,
                    s.ParentId
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy học sinh." });
            }

            return Ok(student);
        }

        [HttpPost("api/center/students")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var parentExists = await _context.Users.AnyAsync(u => u.Id == dto.ParentId && u.Role == "Parent");
            if (!parentExists)
            {
                return BadRequest(new { message = "Phụ huynh được chọn không hợp lệ." });
            }

            var student = new Student
            {
                ParentId = dto.ParentId,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                CreatedAt = DateTime.Now
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, new { id = student.Id, fullName = student.FullName });
        }

        [HttpPut("api/center/students/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentUpdateApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy học sinh." });
            }

            var parentExists = await _context.Users.AnyAsync(u => u.Id == dto.ParentId && u.Role == "Parent");
            if (!parentExists)
            {
                return BadRequest(new { message = "Phụ huynh được chọn không hợp lệ." });
            }

            student.ParentId = dto.ParentId;
            student.FullName = dto.FullName;
            student.DateOfBirth = dto.DateOfBirth;
            student.Gender = dto.Gender;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("api/center/students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy học sinh." });
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class StudentCreateApiDto
    {
        public int ParentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    public class StudentUpdateApiDto
    {
        public int ParentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}

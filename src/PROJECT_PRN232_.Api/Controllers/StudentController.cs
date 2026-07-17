using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var students = await (
                from s in _context.Students
                join u in _context.Users on s.ParentId equals u.Id into parentGroup
                from parent in parentGroup.DefaultIfEmpty()
                orderby s.CreatedAt ascending
                select new
                {
                    s.Id,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender,
                    s.ParentId,
                    ParentName = parent != null ? parent.FullName : "Chưa có",
                    s.CreatedAt,
                    ClassName = (
                        from cs in _context.ClassStudents
                        join c in _context.Classes on cs.ClassId equals c.Id
                        where cs.StudentId == s.Id
                        select c.ClassName
                    ).FirstOrDefault() ?? ""
                }
            ).ToListAsync();

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

            if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value > DateTime.Today)
            {
                return BadRequest(new { message = "Ngày sinh không được vượt quá ngày hiện tại." });
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

            if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value > DateTime.Today)
            {
                return BadRequest(new { message = "Ngày sinh không được vượt quá ngày hiện tại." });
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
        [Required(ErrorMessage = "Phụ huynh liên kết không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Phụ huynh được chọn không hợp lệ.")]
        public int ParentId { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơưẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼẾỀỂỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪỬỮỰỲÝỶỸỴạảấầẩẫậắằẳẵặẹẻẽếềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳýỷỹỵ\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [RegularExpression(@"^(Male|Female)$", ErrorMessage = "Giới tính phải là Nam hoặc Nữ.")]
        public string? Gender { get; set; }
    }

    public class StudentUpdateApiDto
    {
        [Required(ErrorMessage = "Phụ huynh liên kết không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Phụ huynh được chọn không hợp lệ.")]
        public int ParentId { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơưẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼẾỀỂỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪỬỮỰỲÝỶỸỴạảấầẩẫậắằẳẵặẹẻẽếềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳýỷỹỵ\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [RegularExpression(@"^(Male|Female)$", ErrorMessage = "Giới tính phải là Nam hoặc Nữ.")]
        public string? Gender { get; set; }
    }
}

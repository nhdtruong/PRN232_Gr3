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
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/center/teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("api/center/teachers/{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _context.Users
                .Where(u => u.Id == id && u.Role == "Teacher")
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.IsActive
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản giáo viên." });
            }

            return Ok(teacher);
        }

        [HttpPost("api/center/teachers")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherCreateApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Username == dto.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại trong hệ thống." });
            }

            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "Teacher",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeacherById), new { id = user.Id }, new { id = user.Id, fullName = user.FullName });
        }

        [HttpPut("api/center/teachers/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] TeacherUpdateApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Teacher")
            {
                return NotFound(new { message = "Không tìm thấy giáo viên để cập nhật." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("api/center/teachers/{id}/toggle-status")]
        public async Task<IActionResult> ToggleTeacherStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Teacher")
            {
                return NotFound(new { message = "Không tìm thấy giáo viên." });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { isActive = user.IsActive, message = user.IsActive ? "Mở khóa tài khoản thành công." : "Khóa tài khoản thành công." });
        }
    }

    public class TeacherCreateApiDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơư\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Tên đăng nhập chỉ được gồm chữ cái và chữ số, không chứa dấu cách.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        [RegularExpression(@"^(0)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0).")]
        public string? Phone { get; set; }
    }

    public class TeacherUpdateApiDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơư\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        [RegularExpression(@"^(0)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0).")]
        public string? Phone { get; set; }
    }
}

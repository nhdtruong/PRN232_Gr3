using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly AppDbContext _context;

        public ParentController(IEnrollmentService enrollmentService, AppDbContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context;
        }

        // --- Client (Parent Role) Endpoints ---
        [HttpGet("api/parent/children/{studentId}/classes")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetChildClasses(int studentId)
        {
            var parentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(parentIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }
            var classes = await _enrollmentService.GetClassesForStudentAsync(studentId, parentId);
            return Ok(classes);
        }

        // --- Administrative (Center Role) CRUD Endpoints ---
        [HttpGet("api/center/parents")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> GetParents()
        {
            var parents = await _context.Users
                .Where(u => u.Role == "Parent")
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

            return Ok(parents);
        }

        [HttpGet("api/center/parents/{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> GetParentById(int id)
        {
            var parent = await _context.Users
                .Where(u => u.Id == id && u.Role == "Parent")
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

            if (parent == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản phụ huynh." });
            }

            return Ok(parent);
        }

        [HttpPost("api/center/parents")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> CreateParent([FromBody] ParentCreateApiDto dto)
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
                Role = "Parent",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParentById), new { id = user.Id }, new { id = user.Id, fullName = user.FullName });
        }

        [HttpPut("api/center/parents/{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] ParentUpdateApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Parent")
            {
                return NotFound(new { message = "Không tìm thấy phụ huynh để cập nhật." });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("api/center/parents/{id}/toggle-status")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> ToggleParentStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Parent")
            {
                return NotFound(new { message = "Không tìm thấy phụ huynh." });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { isActive = user.IsActive, message = user.IsActive ? "Mở khóa tài khoản thành công." : "Khóa tài khoản thành công." });
        }
    }

    public class ParentCreateApiDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class ParentUpdateApiDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}

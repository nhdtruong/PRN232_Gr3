using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public async Task<IActionResult> GetParents([FromQuery] int? classId = null)
        {
            IQueryable<User> query = _context.Users.Where(u => u.Role == "Parent");

            if (classId.HasValue && classId.Value > 0)
            {
                // Lấy danh sách studentId trong lớp
                var studentIdsInClass = await _context.ClassStudents
                    .Where(cs => cs.ClassId == classId.Value)
                    .Select(cs => cs.StudentId)
                    .ToListAsync();

                // Lấy parentId của các học sinh đó
                var parentIdsInClass = await _context.Students
                    .Where(s => studentIdsInClass.Contains(s.Id))
                    .Select(s => s.ParentId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(u => parentIdsInClass.Contains(u.Id));
            }

            var parents = await query
                .OrderBy(u => u.CreatedAt)
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
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơưẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼẾỀỂỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪỬỮỰỲÝỶỸỴạảấầẩẫậắằẳẵặẹẻẽếềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳýỷỹỵ\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
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

        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng đầu số di động VN như 03, 05, 07, 08, 09).")]
        public string? Phone { get; set; }
    }

    public class ParentUpdateApiDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂÂÊÔƠƯưăâêôơưẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼẾỀỂỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪỬỮỰỲÝỶỸỴạảấầẩẫậắằẳẵặẹẻẽếềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳýỷỹỵ\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng đầu số di động VN như 03, 05, 07, 08, 09).")]
        public string? Phone { get; set; }
    }
}

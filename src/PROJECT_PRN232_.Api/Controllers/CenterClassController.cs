using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize(Roles = "Center")]
    public class CenterClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CenterClassController(
            IClassService classService,
            IEnrollmentService enrollmentService,
            AppDbContext context,
            INotificationService notificationService)
        {
            _classService = classService;
            _enrollmentService = enrollmentService;
            _context = context;
            _notificationService = notificationService;
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
        [HttpPost("api/center/classes")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateWithScheduleDto req)
        {
            if (req == null || req.CreateDto == null)
            {
                return BadRequest(new { message = "Dữ liệu lớp học không hợp lệ." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Kiểm tra sức chứa phòng học
            if (req.CreateRoomId > 0)
            {
                var room = await _context.Rooms.FindAsync(req.CreateRoomId);
                if (room != null && req.CreateDto.MaxCapacity > room.Capacity)
                {
                    return BadRequest(new { message = $"Sĩ số tối đa của lớp ({req.CreateDto.MaxCapacity}) vượt quá sức chứa của phòng {room.RoomName} ({room.Capacity} chỗ)." });
                }
            }

            // Lấy thông tin môn học để sinh số buổi tương ứng
            var subjectObj = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectCode == req.CreateDto.Subject);
            if (subjectObj == null)
            {
                return BadRequest(new { message = "Môn học không tồn tại." });
            }

            int totalLessons = subjectObj.NumberOfSessions;
            req.CreateDto.TotalLessons = totalLessons;

            // Lưu lớp học
            var created = await _classService.CreateClassAsync(req.CreateDto);

            // Sinh các buổi học tuần tự bắt đầu từ hôm nay
            if (created != null)
            {
                for (int i = 0; i < totalLessons; i++)
                {
                    var lesson = new Lesson
                    {
                        ClassId = created.Id,
                        Title = $"Buổi {i + 1}",
                        Description = $"Bài học buổi thứ {i + 1} của lớp {created.ClassName}",
                        LessonDate = DateTime.Today.AddDays(i),
                        RoomId = req.CreateRoomId > 0 ? req.CreateRoomId : (int?)null,
                        SlotId = null,
                        IsPublished = false
                    };
                    _context.Lessons.Add(lesson);
                }
                await _context.SaveChangesAsync();

                // Gửi thông báo phân công giảng dạy cho Giáo viên
                if (created.TeacherId.HasValue && created.TeacherId.Value > 0)
                {
                    try
                    {
                        await _notificationService.NotifyTeacherAssignedClassAsync(created.TeacherId.Value, created.Id, created.ClassName);
                    }
                    catch (Exception) { /* Ignored */ }
                }
            }

            return CreatedAtAction(nameof(GetClassById), new { classId = created.Id }, created);
        }

        [HttpPut("api/center/classes/{classId}")]
        public async Task<IActionResult> UpdateClass(int classId, [FromBody] ClassUpdateWithScheduleDto req)
        {
            if (req == null || req.UpdateDto == null || classId != req.UpdateDto.Id)
            {
                return BadRequest(new { message = "Mã lớp học trong URL và Body không khớp hoặc không hợp lệ." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Kiểm tra sức chứa phòng học
            if (req.UpdateRoomId > 0)
            {
                var room = await _context.Rooms.FindAsync(req.UpdateRoomId);
                if (room != null && req.UpdateDto.MaxCapacity > room.Capacity)
                {
                    return BadRequest(new { message = $"Sĩ số tối đa của lớp ({req.UpdateDto.MaxCapacity}) vượt quá sức chứa của phòng {room.RoomName} ({room.Capacity} chỗ)." });
                }
            }

            // Lấy thông tin môn học để sinh số buổi tương ứng
            var subjectObj = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectCode == req.UpdateDto.Subject);
            if (subjectObj == null)
            {
                return BadRequest(new { message = "Môn học không tồn tại." });
            }

            int totalLessons = subjectObj.NumberOfSessions;
            req.UpdateDto.TotalLessons = totalLessons;

            // Lấy thực thể trước khi cập nhật để so sánh môn học
            var classEntity = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);
            if (classEntity == null)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId} để cập nhật." });
            }

            var updated = await _classService.UpdateClassAsync(req.UpdateDto);
            if (!updated)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId} để cập nhật." });
            }

            // Gửi thông báo phân công giảng dạy cho Giáo viên nếu có thay đổi giáo viên
            if (req.UpdateDto.TeacherId.HasValue && req.UpdateDto.TeacherId.Value > 0 &&
                (classEntity == null || classEntity.TeacherId != req.UpdateDto.TeacherId.Value))
            {
                try
                {
                    await _notificationService.NotifyTeacherAssignedClassAsync(req.UpdateDto.TeacherId.Value, classId, req.UpdateDto.ClassName);
                }
                catch (Exception) { /* Ignored */ }
            }

            // Nếu thay đổi Môn học => Sinh lại số buổi học tương ứng
            if (classEntity.Subject != req.UpdateDto.Subject)
            {
                // Xóa các buổi học cũ (phải xóa các bảng con trước để tránh lỗi FK)
                var existingLessons = _context.Lessons.Where(l => l.ClassId == classId).ToList();
                var lessonIds = existingLessons.Select(l => l.Id).ToList();

                if (lessonIds.Any())
                {
                    // 1. Xóa Materials (tài liệu) thuộc các buổi học này
                    var relatedMaterials = _context.Materials.Where(m => m.LessonId.HasValue && lessonIds.Contains(m.LessonId.Value));
                    _context.Materials.RemoveRange(relatedMaterials);

                    // 2. Xóa Attendances (điểm danh) thuộc các buổi học này
                    var relatedAttendances = _context.Attendances.Where(a => lessonIds.Contains(a.LessonId));
                    _context.Attendances.RemoveRange(relatedAttendances);

                    // 3. Xóa DailyAssessments (đánh giá thường xuyên) thuộc các buổi học này nếu có
                    var relatedAssessments = _context.DailyAssessments.Where(a => lessonIds.Contains(a.LessonId));
                    _context.DailyAssessments.RemoveRange(relatedAssessments);

                    // 4. Cuối cùng mới xóa Lessons
                    _context.Lessons.RemoveRange(existingLessons);
                }

                await _context.SaveChangesAsync();

                // Sinh các buổi học mới tuần tự bắt đầu từ hôm nay
                for (int i = 0; i < totalLessons; i++)
                {
                    var lesson = new Lesson
                    {
                        ClassId = classId,
                        Title = $"Buổi {i + 1}",
                        Description = $"Bài học buổi thứ {i + 1} của lớp {req.UpdateDto.ClassName}",
                        LessonDate = DateTime.Today.AddDays(i),
                        RoomId = req.UpdateRoomId > 0 ? req.UpdateRoomId : (int?)null,
                        SlotId = null,
                        IsPublished = false
                    };
                    _context.Lessons.Add(lesson);
                }
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("api/center/classes/{classId}")]
        public async Task<IActionResult> DeleteClass(int classId)
        {
            var success = await _classService.DeleteClassAsync(classId);
            if (!success)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId} để xóa." });
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
            var classObj = await _context.Classes.FindAsync(classId);
            if (classObj == null)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId}." });
            }

            var className = classObj.ClassName;

            var students = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Student)
                    .ThenInclude(s => s.Parent)
                .Select(cs => new
                {
                    cs.Student.Id,
                    cs.Student.FullName,
                    cs.Student.DateOfBirth,
                    cs.Student.Gender,
                    cs.Student.ParentId,
                    ParentName = cs.Student.Parent != null ? cs.Student.Parent.FullName : "Chưa có",
                    cs.Student.CreatedAt,
                    ClassName = className
                })
                .ToListAsync();

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

        [HttpGet("api/center/classes/{classId}/schedules")]
        public async Task<IActionResult> GetClassSchedules(int classId)
        {
            var classObj = await _context.Classes.FindAsync(classId);
            if (classObj == null)
            {
                return NotFound(new { message = "Không tìm thấy lớp học" });
            }

            var allLessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .OrderBy(l => l.LessonDate)
                .Select(l => new
                {
                    l.Id,
                    l.RoomId,
                    l.SlotId,
                    LessonDate = l.LessonDate,
                    DayOfWeek = (int)l.LessonDate.DayOfWeek
                })
                .ToListAsync();

            var firstRoomId = allLessons.FirstOrDefault(l => l.RoomId.HasValue)?.RoomId;

            var uniqueSchedules = allLessons
                .GroupBy(l => new { l.DayOfWeek, l.SlotId })
                .Select(g => g.First())
                .Select(l => new
                {
                    l.RoomId,
                    l.SlotId,
                    l.DayOfWeek
                })
                .ToList();

            return Ok(new
            {
                classId = classId,
                className = classObj.ClassName,
                centerId = classObj.CenterId,
                maxCapacity = classObj.MaxCapacity,
                totalLessons = classObj.TotalLessons,
                roomId = firstRoomId,
                subject = classObj.Subject,
                teacherId = classObj.TeacherId,
                lessons = uniqueSchedules
            });
        }

        /// <summary>
        /// Tạo mới Học sinh (kèm tuỳ chọn tạo Phụ huynh mới) và xếp thẳng vào lớp.
        /// POST /api/center/classes/{classId}/students/create-and-enroll
        /// </summary>
        [HttpPost("api/center/classes/{classId}/students/create-and-enroll")]
        public async Task<IActionResult> CreateStudentAndEnroll(int classId, [FromBody] CreateStudentAndEnrollDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra lớp học tồn tại
            var classExists = await _context.Classes.AnyAsync(c => c.Id == classId);
            if (!classExists)
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId}." });

            int parentId;

            if (dto.IsNewParent)
            {
                // Validate thông tin phụ huynh mới
                if (string.IsNullOrWhiteSpace(dto.ParentFullName) ||
                    string.IsNullOrWhiteSpace(dto.ParentUsername) ||
                    string.IsNullOrWhiteSpace(dto.ParentPassword))
                {
                    return BadRequest(new { message = "Khi tạo phụ huynh mới, cần cung cấp Họ tên, Tên đăng nhập và Mật khẩu." });
                }

                // Kiểm tra username trùng
                var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.ParentUsername);
                if (usernameExists)
                    return BadRequest(new { message = $"Tên đăng nhập '{dto.ParentUsername}' đã tồn tại trong hệ thống." });

                var newParent = new User
                {
                    FullName     = dto.ParentFullName,
                    Username     = dto.ParentUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.ParentPassword),
                    Email        = dto.ParentEmail,
                    Phone        = dto.ParentPhone,
                    Role         = "Parent",
                    IsActive     = true,
                    CreatedAt    = DateTime.Now
                };

                _context.Users.Add(newParent);
                await _context.SaveChangesAsync();
                parentId = newParent.Id;
            }
            else
            {
                // Dùng phụ huynh có sẵn
                if (dto.ExistingParentId == null || dto.ExistingParentId <= 0)
                    return BadRequest(new { message = "Vui lòng cung cấp ExistingParentId hợp lệ khi IsNewParent = false." });

                var parentExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.ExistingParentId && u.Role == "Parent");
                if (!parentExists)
                    return BadRequest(new { message = "Phụ huynh được chọn không tồn tại hoặc không có Role là Parent." });

                parentId = dto.ExistingParentId.Value;
            }

            // Tạo học sinh
            var newStudent = new Student
            {
                FullName    = dto.StudentFullName,
                DateOfBirth = dto.DateOfBirth,
                ParentId    = parentId,
                CreatedAt   = DateTime.Now
            };

            _context.Students.Add(newStudent);
            await _context.SaveChangesAsync();

            // Xếp học sinh vào lớp
            var enrolled = await _enrollmentService.EnrollStudentAsync(classId, newStudent.Id);
            if (!enrolled)
            {
                return BadRequest(new
                {
                    message = "Đã tạo học sinh nhưng không thể xếp vào lớp (học sinh có thể đã ở lớp này).",
                    studentId = newStudent.Id
                });
            }

            return Ok(new
            {
                message   = "Tạo học sinh và xếp lớp thành công.",
                studentId = newStudent.Id,
                studentName = newStudent.FullName,
                parentId,
                classId
            });
        }
    }

    public class ClassCreateWithScheduleDto
    {
        public ClassCreateDto CreateDto { get; set; } = null!;
        public int CreateRoomId { get; set; }
        public List<string>? DayOfWeek { get; set; }
        public List<int>? SlotId { get; set; }
    }

    public class ClassUpdateWithScheduleDto
    {
        public ClassUpdateDto UpdateDto { get; set; } = null!;
        public int UpdateRoomId { get; set; }
        public List<string>? DayOfWeek { get; set; }
        public List<int>? SlotId { get; set; }
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

    public class CreateStudentAndEnrollDto
    {
        // Thông tin học sinh
        [Required(ErrorMessage = "Họ tên học sinh không được để trống.")]
        public string StudentFullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        // Nếu IsNewParent = false → dùng ExistingParentId
        public bool IsNewParent { get; set; }

        public int? ExistingParentId { get; set; }

        // Nếu IsNewParent = true → tạo Parent mới từ các field dưới
        public string? ParentFullName { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9_]{3,50}$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số và _, độ dài 3-50.")]
        public string? ParentUsername { get; set; }

        public string? ParentPassword { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? ParentEmail { get; set; }

        public string? ParentPhone { get; set; }
    }
}

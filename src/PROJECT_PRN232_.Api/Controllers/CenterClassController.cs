using System;
using System.Collections.Generic;
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

        public CenterClassController(IClassService classService, IEnrollmentService enrollmentService, AppDbContext context)
        {
            _classService = classService;
            _enrollmentService = enrollmentService;
            _context = context;
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

            // Parse Lịch học
            var scheduleList = new List<(DayOfWeek day, int slotId)>();
            var targetDaysMap = new Dictionary<string, DayOfWeek>
            {
                { "Thứ 2", DayOfWeek.Monday },
                { "Thứ 3", DayOfWeek.Tuesday },
                { "Thứ 4", DayOfWeek.Wednesday },
                { "Thứ 5", DayOfWeek.Thursday },
                { "Thứ 6", DayOfWeek.Friday },
                { "Thứ 7", DayOfWeek.Saturday },
                { "Chủ nhật", DayOfWeek.Sunday }
            };

            if (req.DayOfWeek != null && req.SlotId != null)
            {
                int count = Math.Min(req.DayOfWeek.Count, req.SlotId.Count);
                for (int i = 0; i < count; i++)
                {
                    if (targetDaysMap.TryGetValue(req.DayOfWeek[i], out DayOfWeek targetDay))
                    {
                        scheduleList.Add((targetDay, req.SlotId[i]));
                    }
                }
            }

            // 2. Kiểm tra trùng lịch phòng học
            if (req.CreateRoomId > 0 && scheduleList.Count > 0)
            {
                int totalLessonsNeeded = req.CreateDto.TotalLessons > 0 ? req.CreateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var checkingDateTemp = startDate;
                var lessonsCreatedChecking = 0;

                while (lessonsCreatedChecking < totalLessonsNeeded)
                {
                    foreach (var schedule in scheduleList)
                    {
                        if (lessonsCreatedChecking >= totalLessonsNeeded) break;
                        if (schedule.day == checkingDateTemp.DayOfWeek)
                        {
                            var hasOverlap = await _context.Lessons
                                .AnyAsync(l => l.RoomId == req.CreateRoomId && l.SlotId == schedule.slotId && l.LessonDate.Date == checkingDateTemp.Date);

                            if (hasOverlap)
                            {
                                var slotObj = await _context.Slots.FindAsync(schedule.slotId);
                                return BadRequest(new { message = $"Trùng lịch học tại phòng: Phòng đã có lớp khác đăng ký vào ngày {checkingDateTemp:dd/MM/yyyy} ở ca học {slotObj?.SlotName ?? schedule.slotId.ToString()}." });
                            }
                            lessonsCreatedChecking++;
                        }
                    }
                    checkingDateTemp = checkingDateTemp.AddDays(1);
                    if (checkingDateTemp > startDate.AddYears(5)) break;
                }
            }

            // Lưu lớp học
            var created = await _classService.CreateClassAsync(req.CreateDto);

            // Sinh các buổi học
            if (created != null && scheduleList.Count > 0)
            {
                int totalLessonsNeeded = req.CreateDto.TotalLessons > 0 ? req.CreateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var currentCheckingDate = startDate;
                var lessonsCreated = 0;

                while (lessonsCreated < totalLessonsNeeded)
                {
                    foreach (var schedule in scheduleList)
                    {
                        if (lessonsCreated >= totalLessonsNeeded) break;
                        if (schedule.day == currentCheckingDate.DayOfWeek)
                        {
                            var lesson = new Lesson
                            {
                                ClassId = created.Id,
                                Title = $"Buổi {lessonsCreated + 1}",
                                Description = $"Bài học buổi thứ {lessonsCreated + 1} của lớp {created.ClassName}",
                                LessonDate = currentCheckingDate,
                                RoomId = req.CreateRoomId > 0 ? req.CreateRoomId : (int?)null,
                                SlotId = schedule.slotId,
                                IsPublished = true
                            };
                            _context.Lessons.Add(lesson);
                            lessonsCreated++;
                        }
                    }
                    currentCheckingDate = currentCheckingDate.AddDays(1);
                    if (currentCheckingDate > startDate.AddYears(5)) break;
                }
                await _context.SaveChangesAsync();
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

            // Parse Lịch học
            var scheduleList = new List<(DayOfWeek day, int slotId)>();
            var targetDaysMap = new Dictionary<string, DayOfWeek>
            {
                { "Thứ 2", DayOfWeek.Monday },
                { "Thứ 3", DayOfWeek.Tuesday },
                { "Thứ 4", DayOfWeek.Wednesday },
                { "Thứ 5", DayOfWeek.Thursday },
                { "Thứ 6", DayOfWeek.Friday },
                { "Thứ 7", DayOfWeek.Saturday },
                { "Chủ nhật", DayOfWeek.Sunday }
            };

            if (req.DayOfWeek != null && req.SlotId != null)
            {
                int count = Math.Min(req.DayOfWeek.Count, req.SlotId.Count);
                for (int i = 0; i < count; i++)
                {
                    if (targetDaysMap.TryGetValue(req.DayOfWeek[i], out DayOfWeek targetDay))
                    {
                        scheduleList.Add((targetDay, req.SlotId[i]));
                    }
                }
            }

            // 2. Kiểm tra trùng lịch phòng học (loại trừ các buổi của chính lớp này)
            if (req.UpdateRoomId > 0 && scheduleList.Count > 0)
            {
                int totalLessonsNeeded = req.UpdateDto.TotalLessons > 0 ? req.UpdateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var checkingDateTemp = startDate;
                var lessonsCreatedChecking = 0;

                while (lessonsCreatedChecking < totalLessonsNeeded)
                {
                    foreach (var schedule in scheduleList)
                    {
                        if (lessonsCreatedChecking >= totalLessonsNeeded) break;
                        if (schedule.day == checkingDateTemp.DayOfWeek)
                        {
                            var hasOverlap = await _context.Lessons
                                .AnyAsync(l => l.ClassId != req.UpdateDto.Id && l.RoomId == req.UpdateRoomId && l.SlotId == schedule.slotId && l.LessonDate.Date == checkingDateTemp.Date);

                            if (hasOverlap)
                            {
                                var slotObj = await _context.Slots.FindAsync(schedule.slotId);
                                return BadRequest(new { message = $"Trùng lịch học tại phòng: Phòng đã có lớp khác đăng ký vào ngày {checkingDateTemp:dd/MM/yyyy} ở ca học {slotObj?.SlotName ?? schedule.slotId.ToString()}." });
                            }
                            lessonsCreatedChecking++;
                        }
                    }
                    checkingDateTemp = checkingDateTemp.AddDays(1);
                    if (checkingDateTemp > startDate.AddYears(5)) break;
                }
            }

            var updated = await _classService.UpdateClassAsync(req.UpdateDto);
            if (!updated)
            {
                return NotFound(new { message = $"Không tìm thấy lớp học ID {classId} để cập nhật." });
            }

            // Xóa các buổi học cũ
            var existingLessons = _context.Lessons.Where(l => l.ClassId == req.UpdateDto.Id);
            _context.Lessons.RemoveRange(existingLessons);
            await _context.SaveChangesAsync();

            // Sinh lịch học mới
            if (scheduleList.Count > 0)
            {
                int totalLessonsNeeded = req.UpdateDto.TotalLessons > 0 ? req.UpdateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var currentCheckingDate = startDate;
                var lessonsCreated = 0;

                while (lessonsCreated < totalLessonsNeeded)
                {
                    foreach (var schedule in scheduleList)
                    {
                        if (lessonsCreated >= totalLessonsNeeded) break;
                        if (schedule.day == currentCheckingDate.DayOfWeek)
                        {
                            var lesson = new Lesson
                            {
                                ClassId = req.UpdateDto.Id,
                                Title = $"Buổi {lessonsCreated + 1}",
                                Description = $"Bài học buổi thứ {lessonsCreated + 1} của lớp {req.UpdateDto.ClassName}",
                                LessonDate = currentCheckingDate,
                                RoomId = req.UpdateRoomId > 0 ? req.UpdateRoomId : (int?)null,
                                SlotId = schedule.slotId,
                                IsPublished = true
                            };
                            _context.Lessons.Add(lesson);
                            lessonsCreated++;
                        }
                    }
                    currentCheckingDate = currentCheckingDate.AddDays(1);
                    if (currentCheckingDate > startDate.AddYears(5)) break;
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

        [HttpGet("api/center/classes/{classId}/lessons")]
        public async Task<IActionResult> GetClassLessons(int classId)
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
                lessons = uniqueSchedules
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Data.Enums;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;

namespace PROJECT_PRN232_.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public LessonService(
            ILessonRepository lessonRepository,
            AppDbContext context,
            INotificationService notificationService)
        {
            _lessonRepository = lessonRepository;
            _context = context;
            _notificationService = notificationService;
        }

        // Lấy danh sách buổi học của Lớp học
        public async Task<IEnumerable<LessonResponseDto>> GetByClassIdAsync(int classId)
        {
            var lessons = await _lessonRepository.GetByClassIdAsync(classId);
            return lessons.Select(MapToDto);
        }

        // Lấy thông tin chi tiết một buổi học
        public async Task<LessonResponseDto?> GetByIdAsync(int lessonId)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return null;
            return MapToDto(lesson);
        }

        // Tạo buổi học mới
        public async Task<LessonResponseDto?> CreateAsync(LessonCreateDto dto, int centerUserId)
        {
            // 1. Kiểm tra Lớp học tồn tại
            var targetClass = await _lessonRepository.GetClassByIdAsync(dto.ClassId);
            if (targetClass == null) return null;

            // 2. Bảo mật: Đảm bảo Center này là chủ sở hữu lớp học
            if (targetClass.CenterId != centerUserId) return null;

            // 3. Quy tắc: Không xếp học sinh / tạo buổi học cho lớp đã đóng
            if (targetClass.Status != "Active") return null;

            var lesson = new Lesson
            {
                ClassId = dto.ClassId,
                Title = dto.Title,
                Description = dto.Description,
                LessonDate = dto.LessonDate
            };

            var created = await _lessonRepository.CreateAsync(lesson);

            // TỰ ĐỘNG TẠO BẢN GHI ĐIỂM DANH MẶC ĐỊNH
            // Lấy danh sách ID học sinh trong lớp
            var studentIds = await _context.ClassStudents
                .Where(cs => cs.ClassId == dto.ClassId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            if (studentIds.Any())
            {
                var defaultAttendances = studentIds.Select(studentId => new Attendance
                {
                    StudentId = studentId,
                    LessonId = created.Id,
                    Status = AttendanceStatus.Present,
                    Note = "Tự động khởi tạo",
                    UpdatedAt = DateTime.Now
                }).ToList();

                _context.Attendances.AddRange(defaultAttendances);
                await _context.SaveChangesAsync();
            }
            
            // Lấy lại buổi học kèm Class navigation property để map đầy đủ thông tin tên Lớp
            var fullLesson = await _lessonRepository.GetLessonWithClassAsync(created.Id);
            return fullLesson != null ? MapToDto(fullLesson) : null;
        }

        // Sửa buổi học
        public async Task<bool> UpdateAsync(int lessonId, LessonUpdateDto dto, int centerUserId)
        {
            var existingLesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (existingLesson == null) return false;

            // Bảo mật: Chỉ Center sở hữu lớp này mới được quyền sửa
            if (existingLesson.Class.CenterId != centerUserId) return false;

            // Bảo mật: Không sửa buổi học của lớp đã đóng
            if (existingLesson.Class.Status != "Active") return false;

            existingLesson.Title = dto.Title;
            existingLesson.Description = dto.Description;
            existingLesson.LessonDate = dto.LessonDate;

            return await _lessonRepository.UpdateAsync(existingLesson);
        }

        // Xóa buổi học
        public async Task<bool> DeleteAsync(int lessonId, int centerUserId)
        {
            var existingLesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (existingLesson == null) return false;

            // Bảo mật: Chỉ Center sở hữu lớp này mới được quyền xóa
            if (existingLesson.Class.CenterId != centerUserId) return false;

            return await _lessonRepository.DeleteAsync(lessonId);
        }

        // Parent xem lịch học của con
        public async Task<IEnumerable<LessonResponseDto>> GetByStudentForParentAsync(int studentId, int parentUserId)
        {
            // Bảo mật: Đảm bảo học sinh này thực sự là con của phụ huynh đăng nhập
            var isOwnChild = await _context.Students
                .AnyAsync(s => s.Id == studentId && s.ParentId == parentUserId);
            
            if (!isOwnChild)
            {
                return Enumerable.Empty<LessonResponseDto>();
            }

            var lessons = await _lessonRepository.GetLessonsByStudentIdAsync(studentId);
            return lessons.Where(l => l.IsPublished).Select(MapToDto);
        }

        // Center xuất bản buổi học và gửi thông báo tổng hợp tới phụ huynh
        public async Task<bool> PublishAsync(int lessonId, int centerUserId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Class)
                .Include(l => l.Materials)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return false;

            // Bảo mật: Center sở hữu lớp này mới được quyền publish
            if (lesson.Class.CenterId != centerUserId) return false;

            bool isRebroadcast = lesson.IsPublished;

            lesson.IsPublished = true;
            await _context.SaveChangesAsync();

            // Tổng hợp danh sách tên tài liệu đã đính kèm
            var materialTitles = lesson.Materials.Select(m => m.Title).ToList();

            // Gửi một thông báo tổng hợp duy nhất qua SignalR
            await _notificationService.NotifyPublishedLessonAsync(
                lesson.Id,
                lesson.ClassId,
                lesson.Class.ClassName,
                lesson.Title,
                lesson.LessonDate,
                materialTitles,
                isRebroadcast);

            return true;
        }

        // Mapper Helper chuyển đổi Entity sang DTO
        private static LessonResponseDto MapToDto(Lesson l) => new()
        {
            Id = l.Id,
            ClassId = l.ClassId,
            ClassName = l.Class?.ClassName ?? string.Empty,
            Title = l.Title,
            Description = l.Description,
            LessonDate = l.LessonDate,
            IsPublished = l.IsPublished
        };
    }
}

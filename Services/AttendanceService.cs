using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Data.Enums;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly AppDbContext _context;

        public AttendanceService(IAttendanceRepository attendanceRepository, ILessonRepository lessonRepository, AppDbContext context)
        {
            _attendanceRepository = attendanceRepository;
            _lessonRepository = lessonRepository;
            _context = context;
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return Enumerable.Empty<AttendanceResponseDto>();

            var enrolledIds = await _lessonRepository.GetEnrolledStudentIdsAsync(lesson.ClassId);
            var attendances = await _attendanceRepository.GetByLessonIdAsync(lessonId);

            return attendances
                .Where(a => enrolledIds.Contains(a.StudentId))
                .Where(a => !parentIdFilter.HasValue || a.Student.ParentId == parentIdFilter.Value)
                .Select(MapToDto);
        }

        public async Task<bool> SaveBulkAsync(int lessonId, LessonAttendanceBulkDto dto, int centerUserId)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null || lesson.Class.CenterId != centerUserId)
                return false;

            var enrolledIds = await _lessonRepository.GetEnrolledStudentIdsAsync(lesson.ClassId);
            if (!ValidateStudentIds(dto.Items.Select(i => i.StudentId), enrolledIds))
                return false;

            var entities = dto.Items.Select(i => new Attendance
            {
                Id = i.Id ?? 0,
                StudentId = i.StudentId,
                LessonId = lessonId,
                Status = i.Status,
                Note = i.Note
            });

            await _attendanceRepository.UpsertBulkAsync(lessonId, entities);
            return true;
        }

        // Lấy lịch sử điểm danh của học sinh (Parent hoặc Center xem)
        public async Task<IEnumerable<AttendanceResponseDto>> GetByStudentIdAsync(int studentId, int? parentIdFilter = null)
        {
            // Bảo mật: Nếu là Phụ huynh, kiểm tra xem học sinh đó có thực sự là con của phụ huynh này không
            if (parentIdFilter.HasValue)
            {
                var isOwnChild = await _context.Students
                    .AnyAsync(s => s.Id == studentId && s.ParentId == parentIdFilter.Value);
                if (!isOwnChild)
                {
                    return Enumerable.Empty<AttendanceResponseDto>();
                }
            }

            var list = await _attendanceRepository.GetByStudentIdAsync(studentId);
            if (parentIdFilter.HasValue)
            {
                list = list.Where(a => a.Lesson != null && a.Lesson.IsPublished);
            }
            return list.Select(MapToDto);
        }

        // Center sửa 1 bản ghi điểm danh
        public async Task<bool> UpdateSingleAsync(int attendanceId, AttendanceUpsertDto dto, int centerUserId)
        {
            // Tìm bản ghi điểm danh và thông tin lớp học liên quan
            var existing = await _context.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Class)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (existing == null) return false;

            // Bảo mật: Chỉ Center sở hữu lớp học của buổi học này mới được quyền sửa
            if (existing.Lesson.Class.CenterId != centerUserId) return false;

            existing.Status = dto.Status;
            existing.Note = dto.Note;

            return await _attendanceRepository.UpdateSingleAsync(existing);
        }

        private static AttendanceResponseDto MapToDto(Attendance a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student?.FullName ?? string.Empty,
            LessonId = a.LessonId,
            LessonTitle = a.Lesson?.Title ?? string.Empty,
            ClassName = a.Lesson?.Class?.ClassName ?? string.Empty,
            Status = a.Status,
            Note = a.Note,
            UpdatedAt = a.UpdatedAt
        };

        private static bool ValidateStudentIds(IEnumerable<int> studentIds, HashSet<int> enrolledIds)
        {
            return studentIds.All(enrolledIds.Contains);
        }
    }
}

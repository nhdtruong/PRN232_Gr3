using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Data.Enums;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;

namespace PROJECT_PRN232_.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILessonRepository _lessonRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository, ILessonRepository lessonRepository)
        {
            _attendanceRepository = attendanceRepository;
            _lessonRepository = lessonRepository;
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

        private static AttendanceResponseDto MapToDto(Attendance a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student.FullName,
            LessonId = a.LessonId,
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

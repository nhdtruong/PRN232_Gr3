using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null);
        Task<bool> SaveBulkAsync(int lessonId, LessonAttendanceBulkDto dto, int centerUserId);
    }
}

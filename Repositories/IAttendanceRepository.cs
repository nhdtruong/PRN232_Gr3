using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetByLessonIdAsync(int lessonId);
        Task<Attendance?> GetByStudentAndLessonAsync(int studentId, int lessonId);
        Task UpsertBulkAsync(int lessonId, IEnumerable<Attendance> items);
    }
}

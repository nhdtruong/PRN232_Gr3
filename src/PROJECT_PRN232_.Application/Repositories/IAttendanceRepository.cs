using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetByLessonIdAsync(int lessonId);
        Task<Attendance?> GetByStudentAndLessonAsync(int studentId, int lessonId);
        Task UpsertBulkAsync(int lessonId, IEnumerable<Attendance> items);
        
        // Phương thức mới cho Người 4
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId);
        Task<bool> UpdateSingleAsync(Attendance attendance);
        Task<Attendance?> GetByIdAsync(int id);
    }
}

using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public interface ILessonRepository
    {
        Task<Lesson?> GetLessonWithClassAsync(int lessonId);
        Task<HashSet<int>> GetEnrolledStudentIdsAsync(int classId);
        Task<IEnumerable<(Student Student, Attendance? Attendance, Assessment? Assessment)>> GetRollCallDataAsync(int lessonId, int classId, int? parentIdFilter = null);
    }
}

using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IAssessmentRepository
    {
        Task<IEnumerable<Assessment>> GetByLessonIdAsync(int lessonId);
        Task<Assessment?> GetByStudentAndLessonAsync(int studentId, int lessonId);
        Task UpsertBulkAsync(int lessonId, IEnumerable<Assessment> items);
        Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId);
    }
}

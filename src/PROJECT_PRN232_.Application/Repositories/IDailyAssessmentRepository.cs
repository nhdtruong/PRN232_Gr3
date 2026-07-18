using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IDailyAssessmentRepository
    {
        Task<IEnumerable<DailyAssessment>> GetByLessonIdAsync(int lessonId);
        Task<DailyAssessment?> GetByStudentAndLessonAsync(int studentId, int lessonId);
        Task UpsertBulkAsync(int lessonId, IEnumerable<DailyAssessment> items);
        Task<IEnumerable<DailyAssessment>> GetByStudentIdAsync(int studentId);
    }
}

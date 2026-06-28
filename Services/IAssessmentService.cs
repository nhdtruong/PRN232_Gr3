using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null);
        Task<bool> SaveBulkAsync(int lessonId, LessonAssessmentBulkDto dto, int centerUserId);
    }
}

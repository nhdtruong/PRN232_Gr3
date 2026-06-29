using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null);
        Task<bool> SaveBulkAsync(int lessonId, LessonAssessmentBulkDto dto, int centerUserId);
        Task<IEnumerable<AssessmentResponseDto>> GetByStudentIdAsync(int studentId, int? parentIdFilter = null);
    }
}

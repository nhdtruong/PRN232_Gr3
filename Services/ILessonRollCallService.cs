using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface ILessonRollCallService
    {
        Task<LessonRollCallResponseDto?> GetRollCallByLessonAsync(int lessonId, int? parentIdFilter = null);
        Task<bool> SaveRollCallAsync(int lessonId, LessonRollCallBulkUpsertDto dto, int centerUserId);
    }
}

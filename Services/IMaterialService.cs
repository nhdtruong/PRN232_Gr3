using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialResponseDto>> GetByLessonIdAsync(int lessonId);
        Task<MaterialResponseDto?> CreateAsync(int lessonId, MaterialCreateDto dto, int centerUserId);
        Task<bool> DeleteAsync(int materialId, int centerUserId);
    }
}

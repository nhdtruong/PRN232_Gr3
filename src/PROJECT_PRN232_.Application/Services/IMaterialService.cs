using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialResponseDto>> GetByLessonIdAsync(int lessonId);
        Task<MaterialResponseDto?> CreateAsync(int lessonId, MaterialCreateDto dto, int centerUserId);
        Task<bool> DeleteAsync(int materialId, int centerUserId);
    }
}

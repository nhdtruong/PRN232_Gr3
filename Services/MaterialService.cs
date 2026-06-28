using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public class MaterialService : IMaterialService
    {
        public MaterialService()
        {
        }

        public Task<IEnumerable<MaterialResponseDto>> GetByLessonIdAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<MaterialResponseDto?> CreateAsync(int lessonId, MaterialCreateDto dto, int centerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int materialId, int centerUserId)
        {
            throw new NotImplementedException();
        }
    }
}

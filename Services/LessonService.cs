using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public class LessonService : ILessonService
    {
        public LessonService()
        {
        }

        public Task<LessonResponseDto?> CreateLessonAsync(LessonCreateDto dto, int centerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LessonResponseDto>> GetAllLessonsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

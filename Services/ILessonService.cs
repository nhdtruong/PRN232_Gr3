using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface ILessonService
    {
        /// <summary>
        /// Tạo một buổi học mới.
        /// Cần kiểm tra: Lớp học phải đang hoạt động (Active), không thuộc lớp đã đóng.
        /// </summary>
        Task<LessonResponseDto?> CreateLessonAsync(LessonCreateDto dto, int centerUserId);

        /// <summary>
        /// Lấy danh sách tất cả các buổi học (Center quản lý, có thể filter theo ngày).
        /// </summary>
        Task<IEnumerable<LessonResponseDto>> GetAllLessonsAsync();
    }
}

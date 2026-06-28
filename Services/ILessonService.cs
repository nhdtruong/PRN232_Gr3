using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface ILessonService
    {
        // Lấy danh sách buổi học của một Lớp
        Task<IEnumerable<LessonResponseDto>> GetByClassIdAsync(int classId);

        // Xem chi tiết một buổi học cụ thể
        Task<LessonResponseDto?> GetByIdAsync(int lessonId);

        // Tạo buổi học mới (Kiểm tra lớp hoạt động và quyền sở hữu của Center)
        Task<LessonResponseDto?> CreateAsync(LessonCreateDto dto, int centerUserId);

        // Sửa buổi học (Kiểm tra quyền sở hữu của Center)
        Task<bool> UpdateAsync(int lessonId, LessonUpdateDto dto, int centerUserId);

        // Xóa buổi học (Kiểm tra quyền sở hữu của Center)
        Task<bool> DeleteAsync(int lessonId, int centerUserId);

        // Parent xem lịch học của con
        Task<IEnumerable<LessonResponseDto>> GetByStudentForParentAsync(int studentId, int parentUserId);

        // Center xuất bản buổi học và gửi thông báo tổng hợp tới phụ huynh
        Task<bool> PublishAsync(int lessonId, int centerUserId);
    }
}

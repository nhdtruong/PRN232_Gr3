using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface ILessonService
    {
        // Lấy danh sách buổi học của một Lớp
        Task<IEnumerable<LessonResponseDto>> GetLessonsByClassIdAsync(int classId);

        // Xem chi tiết một buổi học cụ thể
        Task<LessonResponseDto?> GetByIdAsync(int lessonId);

        // Tạo buổi học mới (Kiểm tra lớp hoạt động và quyền sở hữu của Teacher)
        Task<LessonResponseDto> CreateAsync(LessonCreateDto dto, int teacherUserId);

        // Sửa buổi học (Kiểm tra quyền sở hữu của Teacher)
        Task<bool> UpdateAsync(LessonUpdateDto dto, int teacherUserId);

        // Xóa buổi học (Kiểm tra quyền sở hữu của Teacher)
        Task<bool> DeleteAsync(int lessonId, int teacherUserId);

        // Parent xem lịch học của con
        Task<IEnumerable<LessonResponseDto>> GetByStudentForParentAsync(int studentId, int parentUserId);

        // Teacher xuất bản buổi học và gửi thông báo tổng hợp tới phụ huynh
        Task<bool> PublishAsync(int lessonId, int teacherUserId);
    }
}

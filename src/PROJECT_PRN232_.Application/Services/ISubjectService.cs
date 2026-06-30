using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface ISubjectService
    {
        /// <summary>
        /// Lấy danh sách tất cả môn học của một trung tâm
        /// </summary>
        Task<IEnumerable<SubjectResponseDto>> GetByCenterIdAsync(int centerId);

        /// <summary>
        /// Xem chi tiết một môn học
        /// </summary>
        Task<SubjectResponseDto?> GetByIdAsync(int subjectId, int centerUserId);

        /// <summary>
        /// Tạo mới môn học (Center)
        /// </summary>
        Task<SubjectResponseDto> CreateAsync(SubjectCreateDto dto, int centerUserId);

        /// <summary>
        /// Cập nhật thông tin môn học (Center)
        /// </summary>
        Task<bool> UpdateAsync(SubjectUpdateDto dto, int centerUserId);

        /// <summary>
        /// Xóa môn học (Center) — tự động xóa luôn tài liệu của môn học đó
        /// </summary>
        Task<bool> DeleteAsync(int subjectId, int centerUserId);
    }
}

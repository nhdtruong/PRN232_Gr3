using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null);
        Task<bool> SaveBulkAsync(int lessonId, LessonAttendanceBulkDto dto, int centerUserId);
        
        /// <summary>
        /// Xem lịch sử điểm danh của một học sinh cụ thể (dành cho Center báo cáo hoặc Parent theo dõi con mình).
        /// </summary>
        Task<IEnumerable<AttendanceResponseDto>> GetByStudentIdAsync(int studentId, int? parentIdFilter = null);

        /// <summary>
        /// Sửa 1 bản ghi điểm danh đơn lẻ (dành cho Center).
        /// </summary>
        Task<bool> UpdateSingleAsync(int attendanceId, AttendanceUpsertDto dto, int centerUserId);
    }
}


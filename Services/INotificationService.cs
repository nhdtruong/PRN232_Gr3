using PROJECT_PRN232_.Data.Enums;
using PROJECT_PRN232_.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Gửi thông báo kết quả học tập & điểm danh (Roll Call, Ghi chú, Điểm số, Nhận xét GV) cho 1 phụ huynh.
        /// </summary>
        Task NotifyRollCallUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string lessonTitle,
            string studentName,
            AttendanceStatus attendanceStatus,
            string? attendanceNote,
            decimal? score,
            string? teacherComment);

        /// <summary>
        /// Thông báo tài liệu mới đến TẤT CẢ phụ huynh có con trong lớp (dùng khi giáo viên "giao bài").
        /// </summary>
        Task NotifyNewMaterialAsync(int classId, string className, string lessonTitle, DateTime lessonDate, string materialTitle);

        /// <summary>Thông báo con được nhập học vào lớp cho 1 phụ huynh cụ thể.</summary>
        Task NotifyStudentEnrolledAsync(int parentId, string studentName, int classId, string className);

        /// <summary>
        /// Thông báo buổi học mới được tạo đến TẤT CẢ phụ huynh có con trong lớp.
        /// </summary>
        Task NotifyNewLessonAsync(int classId, string className, string lessonTitle, DateTime lessonDate);

        Task<List<NotificationResponseDto>> GetLatestNotificationsByParentAsync(int parentId, int limit);
        Task MarkAllAsReadByParentAsync(int parentId);

        /// <summary>
        /// Đánh dấu 1 thông báo cụ thể là đã đọc. Trả về số thông báo chưa đọc mới sau khi đã cập nhật.
        /// </summary>
        Task<int> MarkSingleAsReadAsync(int notificationId, int parentId);

        /// <summary>
        /// Gửi thông báo tổng hợp (Lớp, Buổi, Thời gian, danh sách tài liệu) khi Center xuất bản buổi học.
        /// </summary>
        Task NotifyPublishedLessonAsync(int lessonId, int classId, string className, string lessonTitle, DateTime lessonDate, List<string> materialTitles);
    }
}

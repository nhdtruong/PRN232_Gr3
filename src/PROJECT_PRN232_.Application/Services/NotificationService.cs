using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly IClassStudentRepository _classStudentRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IRealtimeNotifier realtimeNotifier,
            IClassStudentRepository classStudentRepository)
        {
            _notificationRepository = notificationRepository;
            _realtimeNotifier = realtimeNotifier;
            _classStudentRepository = classStudentRepository;
        }

        // ── Thông báo Kết quả học tập & Điểm danh (RollCall, Ghi chú, Điểm, Nhận xét) ──
        public async Task NotifyRollCallUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string lessonTitle,
            string studentName,
            AttendanceStatus attendanceStatus,
            string? attendanceNote,
            decimal? score,
            string? teacherComment)
        {
            var statusText = attendanceStatus switch
            {
                AttendanceStatus.Present => "<span class='badge bg-success'>Có mặt</span>",
                AttendanceStatus.Absent => "<span class='badge bg-danger'>Vắng mặt</span>",
                AttendanceStatus.Late => "<span class='badge bg-warning text-dark'>Đi trễ</span>",
                AttendanceStatus.Excused => "<span class='badge bg-info text-dark'>Vắng có phép</span>",
                _ => "<span class='badge bg-secondary'>Chưa điểm danh</span>"
            };

            var scoreText = score.HasValue 
                ? $"<strong class='text-primary' style='font-size: 1.05rem;'>{score.Value.ToString("0.##")}</strong> / 10" 
                : "<span class='text-muted'>Chưa có điểm</span>";

            var noteText = !string.IsNullOrWhiteSpace(attendanceNote) 
                ? $"<span class='text-dark fw-normal'>{attendanceNote}</span>" 
                : "<span class='text-muted' style='font-style: italic;'>Không có ghi chú</span>";

            var commentText = !string.IsNullOrWhiteSpace(teacherComment) 
                ? $"<span class='text-dark fw-normal'>{teacherComment}</span>" 
                : "<span class='text-muted' style='font-style: italic;'>Không có nhận xét</span>";

            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='mb-2'><b>Điểm danh:</b> {statusText}</div>
<div class='mb-2'><b>Ghi chú điểm danh:</b> {noteText}</div>
<div class='mb-2'><b>Điểm số:</b> {scoreText}</div>
<div class='mb-1'><b>Nhận xét của giáo viên:</b></div>
<div class='p-2 bg-light rounded border-start border-primary border-3' style='font-size: 0.85rem;'>{commentText}</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Kết quả học tập & Điểm danh - {studentName}",
                Message = messageBody,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

            var notificationDto = new NotificationResponseDto
            {
                Id = notification.Id,
                ClassId = notification.ClassId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _realtimeNotifier.PushNotificationToParentAsync(parentId, notificationDto, unreadCount);
        }

        // ── Thông báo tài liệu mới đến TẤT CẢ phụ huynh trong lớp ────────────
        public async Task NotifyNewMaterialAsync(int classId, string className, string lessonTitle, DateTime lessonDate, string materialTitle)
        {
            var parentIds = await _classStudentRepository.GetParentIdsInClassAsync(classId);

            var dateStr = lessonDate.ToString("HH:mm - dd/MM/yyyy");
            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<div class='mb-2'><b>Thời gian lớp học:</b> <span class='badge bg-primary'>{dateStr}</span></div>
";

            foreach (var parentId in parentIds)
            {
                if (parentId == 0) continue;

                var notification = new Notification
                {
                    ParentId = parentId,
                    ClassId = classId,
                    Title = $"Tài liệu mới - {className}",
                    Message = messageBody,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                await _notificationRepository.AddAsync(notification);
                var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

                var dto = new NotificationResponseDto
                {
                    Id = notification.Id,
                    ClassId = notification.ClassId,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };

                await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
            }
        }

        // ── Thông báo con được nhập học vào lớp (1 phụ huynh cụ thể) ──────────
        public async Task NotifyStudentEnrolledAsync(int parentId, string studentName, int classId, string className)
        {
            if (parentId == 0) return;

            var messageBody = $@"
<div class='mb-2'><b>Học sinh:</b> {studentName}</div>
<div class='mb-2'><b>Lớp học:</b> <span class='badge bg-indigo text-white' style='background-color: #4F46E5;'>{className}</span></div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='text-muted small' style='font-size: 0.8rem;'>Chào mừng học sinh tham gia lớp học! Phụ huynh có thể bắt đầu theo dõi lịch học tại mục <b>Lớp của con</b>.</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Nhập học thành công - {studentName}",
                Message = messageBody,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

            var dto = new NotificationResponseDto
            {
                Id = notification.Id,
                ClassId = notification.ClassId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
        }

        // ── Thông báo con bị xóa/rút khỏi lớp (1 phụ huynh cụ thể) ──────────
        public async Task NotifyStudentRemovedAsync(int parentId, string studentName, int classId, string className)
        {
            if (parentId == 0) return;

            var messageBody = $@"
<div class='mb-2'><b>Học sinh:</b> {studentName}</div>
<div class='mb-2'><b>Lớp học:</b> <span class='badge bg-danger text-white'>{className}</span></div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='text-muted small' style='font-size: 0.8rem;'>Học sinh đã được rút khỏi lớp học này. Vui lòng liên hệ với trung tâm nếu phụ huynh có bất kỳ thắc mắc nào.</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Rút khỏi lớp học - {studentName}",
                Message = messageBody,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

            var dto = new NotificationResponseDto
            {
                Id = notification.Id,
                ClassId = notification.ClassId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
        }

        // ── Thông báo con được chuyển lớp (1 phụ huynh cụ thể) ──────────
        public async Task NotifyStudentTransferredAsync(int parentId, string studentName, int fromClassId, string fromClassName, int toClassId, string toClassName)
        {
            if (parentId == 0) return;

            var messageBody = $@"
<div class='mb-2'><b>Học sinh:</b> {studentName}</div>
<div class='mb-2'><b>Từ lớp:</b> <span class='badge bg-secondary text-white'>{fromClassName}</span></div>
<div class='mb-2'><b>Sang lớp:</b> <span class='badge bg-indigo text-white' style='background-color: #4F46E5;'>{toClassName}</span></div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='text-muted small' style='font-size: 0.8rem;'>Học sinh đã được chuyển lớp thành công. Phụ huynh có thể bắt đầu theo dõi lịch học mới tại mục <b>Lớp của con</b>.</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = toClassId,
                Title = $"Chuyển lớp thành công - {studentName}",
                Message = messageBody,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

            var dto = new NotificationResponseDto
            {
                Id = notification.Id,
                ClassId = notification.ClassId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
        }

        // ── Thông báo buổi học mới đến TẤT CẢ phụ huynh trong lớp ────────────
        public async Task NotifyNewLessonAsync(int classId, string className, string lessonTitle, DateTime lessonDate)
        {
            var parentIds = await _classStudentRepository.GetParentIdsInClassAsync(classId);

            var dateStr = lessonDate.ToString("HH:mm - dd/MM/yyyy");

            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<div class='mb-2'><b>Thời gian lớp học:</b> <span class='badge bg-primary'>{dateStr}</span></div>
";

            foreach (var parentId in parentIds)
            {
                if (parentId == 0) continue;

                var notification = new Notification
                {
                    ParentId = parentId,
                    ClassId = classId,
                    Title = $"Buổi học mới - {className}",
                    Message = messageBody,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                await _notificationRepository.AddAsync(notification);
                var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

                var dto = new NotificationResponseDto
                {
                    Id = notification.Id,
                    ClassId = notification.ClassId,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };

                await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
            }
        }

        // ── Lấy danh sách thông báo cho Parent ────────────────────────────────
        public async Task<List<NotificationResponseDto>> GetLatestNotificationsByParentAsync(int parentId, int limit)
        {
            var notifications = await _notificationRepository.GetLatestNotificationsByParentAsync(parentId, limit);
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                ClassId = n.ClassId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        // ── Đánh dấu tất cả đã đọc ────────────────────────────────────────────
        public async Task MarkAllAsReadByParentAsync(int parentId)
        {
            await _notificationRepository.MarkAllAsReadByParentAsync(parentId);
        }

        // ── Đánh dấu 1 thông báo đã đọc → trả về unreadCount mới ─────────────
        public async Task<int> MarkSingleAsReadAsync(int notificationId, int parentId)
        {
            await _notificationRepository.MarkSingleAsReadAsync(notificationId, parentId);
            return await _notificationRepository.CountUnreadByParentAsync(parentId);
        }

        // ── Gửi thông báo tổng hợp khi xuất bản buổi học ────────────────────
        public async Task NotifyPublishedLessonAsync(int lessonId, int classId, string className, string lessonTitle, DateTime lessonDate, List<string> materialTitles, bool isRebroadcast = false)
        {
            var parentIds = await _classStudentRepository.GetParentIdsInClassAsync(classId);

            var dateStr = lessonDate.ToString("HH:mm - dd/MM/yyyy");

            foreach (var parentId in parentIds)
            {
                if (parentId == 0) continue;

                var studentInClass = await _classStudentRepository.GetStudentInClassForParentAsync(classId, parentId);
                var studentId = studentInClass?.Id ?? 0;

                var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<div class='mb-2'><b>Thời gian lớp học:</b> <span class='badge bg-primary'>{dateStr}</span></div>
<div class='text-center mt-3'>
    <a href='/Parent/Lessons?ChildId={studentId}&LessonId={lessonId}' 
       class='btn btn-sm px-4 fw-semibold text-white d-inline-block' 
       style='background: linear-gradient(135deg, #4F46E5, #7C3AED); border: none; border-radius: 8px; font-size: 0.82rem; text-decoration: none; padding: 6px 16px; box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25);'>
        <i class='bi bi-folder2-open me-1'></i> Xem tài liệu học tập
    </a>
</div>
";

                var notification = new Notification
                {
                    ParentId = parentId,
                    ClassId = classId,
                    Title = isRebroadcast ? $"Cập nhật buổi học - {className}" : $"Buổi học mới - {className}",
                    Message = messageBody,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                await _notificationRepository.AddAsync(notification);
                var unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);

                var dto = new NotificationResponseDto
                {
                    Id = notification.Id,
                    ClassId = notification.ClassId,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };

                await _realtimeNotifier.PushNotificationToParentAsync(parentId, dto, unreadCount);
            }
        }
    }
}

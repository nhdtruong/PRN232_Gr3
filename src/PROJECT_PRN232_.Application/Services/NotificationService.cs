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
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IDailyAssessmentRepository _assessmentRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IClassTranscriptRepository _transcriptRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IRealtimeNotifier realtimeNotifier,
            IClassStudentRepository classStudentRepository,
            IAttendanceRepository attendanceRepository,
            IDailyAssessmentRepository assessmentRepository,
            ILessonRepository lessonRepository,
            IClassTranscriptRepository transcriptRepository)
        {
            _notificationRepository = notificationRepository;
            _realtimeNotifier = realtimeNotifier;
            _classStudentRepository = classStudentRepository;
            _attendanceRepository = attendanceRepository;
            _assessmentRepository = assessmentRepository;
            _lessonRepository = lessonRepository;
            _transcriptRepository = transcriptRepository;
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

        // ── Thông báo Điểm danh riêng biệt ──
        public async Task NotifyAttendanceUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string lessonTitle,
            string studentName,
            AttendanceStatus attendanceStatus,
            string? attendanceNote)
        {
            var statusText = attendanceStatus switch
            {
                AttendanceStatus.Present => "<span class='badge bg-success'>Có mặt</span>",
                AttendanceStatus.Absent => "<span class='badge bg-danger'>Vắng mặt</span>",
                AttendanceStatus.Late => "<span class='badge bg-warning text-dark'>Đi trễ</span>",
                AttendanceStatus.Excused => "<span class='badge bg-info text-dark'>Vắng có phép</span>",
                _ => "<span class='badge bg-secondary'>Chưa điểm danh</span>"
            };

            var noteText = !string.IsNullOrWhiteSpace(attendanceNote)
                ? $"<span class='text-dark fw-normal'>{attendanceNote}</span>"
                : "<span class='text-muted' style='font-style: italic;'>Không có ghi chú</span>";

            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='mb-2'><b>Điểm danh:</b> {statusText}</div>
<div class='mb-2'><b>Ghi chú điểm danh:</b> {noteText}</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Điểm danh - {studentName}",
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

        // ── Thông báo Kết quả học tập riêng biệt ──
        public async Task NotifyGradeUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string lessonTitle,
            string studentName,
            decimal? score,
            string? teacherComment)
        {
            var scoreText = score.HasValue
                ? $"<strong class='text-primary' style='font-size: 1.05rem;'>{score.Value.ToString("0.##")}</strong> / 10"
                : "<span class='text-muted'>Chưa có điểm</span>";

            var commentText = !string.IsNullOrWhiteSpace(teacherComment)
                ? $"<span class='text-dark fw-normal'>{teacherComment}</span>"
                : "<span class='text-muted' style='font-style: italic;'>Không có nhận xét</span>";

            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> {className}</div>
<div class='mb-2'><b>Buổi học:</b> {lessonTitle}</div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='mb-2'><b>Điểm số:</b> {scoreText}</div>
<div class='mb-1'><b>Nhận xét của giáo viên:</b></div>
<div class='p-2 bg-light rounded border-start border-primary border-3' style='font-size: 0.85rem;'>{commentText}</div>
";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Kết quả học tập - {studentName}",
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

                // Lấy thông tin học sinh
                var student = await _classStudentRepository.GetStudentInClassForParentAsync(classId, parentId);
                if (student == null) continue;

                var studentId = student.Id;
                var studentName = student.FullName;

                // 1. Chuyên cần
                var attendance = await _attendanceRepository.GetByStudentAndLessonAsync(studentId, lessonId);

                string attendanceHtml = "";
                if (attendance != null)
                {
                    var statusText = attendance.Status switch
                    {
                        AttendanceStatus.Present => "<span class='badge bg-success'>Có mặt</span>",
                        AttendanceStatus.Absent => "<span class='badge bg-danger'>Vắng mặt</span>",
                        AttendanceStatus.Late => "<span class='badge bg-warning text-dark'>Đi trễ</span>",
                        AttendanceStatus.Excused => "<span class='badge bg-info text-dark'>Vắng có phép</span>",
                        _ => "<span class='badge bg-secondary'>Chưa điểm danh</span>"
                    };
                    var noteText = !string.IsNullOrWhiteSpace(attendance.Note) ? $" ({attendance.Note})" : "";
                    attendanceHtml = $"<div><b>Trạng thái:</b> {statusText}{noteText}</div>";
                }
                else
                {
                    attendanceHtml = "<div class='text-muted' style='font-style: italic;'>Chưa ghi nhận điểm danh</div>";
                }

                // 2. Điểm số hôm nay (Thường xuyên)
                var scoreObj = await _assessmentRepository.GetByStudentAndLessonAsync(studentId, lessonId);

                string scoreHtml = "";
                if (scoreObj != null && scoreObj.Score.HasValue)
                {
                    var commentText = !string.IsNullOrWhiteSpace(scoreObj.Comment)
                        ? $"<div class='mt-1 text-muted small'>Nhận xét: {scoreObj.Comment}</div>"
                        : "";
                    scoreHtml = $"<div><b>Điểm số:</b> <strong class='text-primary'>{scoreObj.Score.Value.ToString("0.##")}</strong> / 10 {commentText}</div>";
                }
                else
                {
                    scoreHtml = "<div class='text-muted' style='font-style: italic;'>Không có điểm số hoặc nhận xét cho buổi này</div>";
                }

                // 3. Điểm giữa kỳ & Cuối kỳ & Điểm TB Thường xuyên (lấy từ ClassTranscript)
                string examGradesHtml = "";
                var transcript = await _transcriptRepository.GetByStudentAndClassAsync(studentId, classId);
                var gkScore = transcript?.MidTermScore;
                var ckScore = transcript?.FinalScore;
                var tbTX = transcript?.AverageDailyScore;
                var finalTotal = transcript?.FinalScoreTotal;

                if (gkScore.HasValue || ckScore.HasValue || tbTX.HasValue)
                {
                    examGradesHtml += $@"
                    <div style='margin-bottom: 16px;'>
                        <div style='font-weight: 700; font-size: 0.9rem; text-transform: uppercase; color: #64748b; margin-bottom: 8px; letter-spacing: 0.05em;'>🏆 Đánh giá & Điểm tổng hợp</div>
                        <div style='padding: 10px; background: #fdfdfd; border-radius: 8px; border: 1px solid #f1f5f9; font-size: 0.85rem;'>";

                    if (tbTX.HasValue)
                    {
                        examGradesHtml += $"<div style='margin-bottom: 4px;'><b>Trung bình Thường xuyên (30%):</b> <strong style='color: #4f46e5;'>{tbTX.Value.ToString("0.##")}</strong> / 10</div>";
                    }
                    if (gkScore.HasValue)
                    {
                        var gkComment = !string.IsNullOrWhiteSpace(transcript?.MidTermComment) ? $" ({transcript.MidTermComment})" : "";
                        examGradesHtml += $"<div style='margin-bottom: 4px;'><b>Điểm Giữa kỳ (30%):</b> <strong style='color: #ea580c;'>{gkScore.Value.ToString("0.##")}</strong> / 10{gkComment}</div>";
                    }
                    if (ckScore.HasValue)
                    {
                        var ckComment = !string.IsNullOrWhiteSpace(transcript?.FinalComment) ? $" ({transcript.FinalComment})" : "";
                        examGradesHtml += $"<div style='margin-bottom: 4px;'><b>Điểm Cuối kỳ (40%):</b> <strong style='color: #16a34a;'>{ckScore.Value.ToString("0.##")}</strong> / 10{ckComment}</div>";
                    }

                    if (finalTotal.HasValue)
                    {
                        examGradesHtml += $"<hr style='opacity: 0.15; margin: 8px 0;'><div style='font-weight: 700; font-size: 0.9rem;'>Tổng kết lớp học: <span class='badge bg-success'>{finalTotal.Value.ToString("0.##")}</span></div>";
                    }

                    examGradesHtml += "</div></div>";
                }

                // 4. Học liệu / Slide bài học
                string materialsHtml = "";
                if (materialTitles != null && materialTitles.Any())
                {
                    var materialList = string.Join("", materialTitles.Select(m => $"<li style='margin-bottom:4px;'>{m}</li>"));
                    materialsHtml = $@"
                    <div style='margin-bottom: 16px;'>
                        <div style='font-weight: 700; font-size: 0.9rem; text-transform: uppercase; color: #64748b; margin-bottom: 8px; letter-spacing: 0.05em;'>📚 Tài liệu & Bài tập buổi học</div>
                        <ul style='padding-left: 20px; margin-bottom: 0; font-size: 0.85rem; color: #475569;'>
                            {materialList}
                        </ul>
                    </div>";
                }

                var messageBody = $@"
<div class='published-lesson-report' style='font-family: inherit; color: #1e293b; background: #fff; border-radius: 12px; padding: 4px; border: none;'>
    <div style='background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); border-radius: 8px; padding: 12px; margin-bottom: 16px;'>
        <div style='margin-bottom: 4px;'><b>Lớp học:</b> {className}</div>
        <div style='margin-bottom: 4px;'><b>Buổi học:</b> {lessonTitle}</div>
        <div><b>Thời gian:</b> <span class='badge bg-primary-subtle text-primary border border-primary-subtle'>{dateStr}</span></div>
    </div>

    <!-- Section 1: Chuyên cần -->
    <div style='margin-bottom: 16px;'>
        <div style='font-weight: 700; font-size: 0.78rem; text-transform: uppercase; color: #64748b; margin-bottom: 6px; letter-spacing: 0.05em;'>📌 Chuyên cần của {studentName}</div>
        <div style='padding: 10px; background: #fff; border-radius: 8px; border: 1px solid #e2e8f0;'>
            {attendanceHtml}
        </div>
    </div>

    <!-- Section 2: Điểm học tập -->
    <div style='margin-bottom: 16px;'>
        <div style='font-weight: 700; font-size: 0.78rem; text-transform: uppercase; color: #64748b; margin-bottom: 6px; letter-spacing: 0.05em;'>📝 Kết quả bài học (Thường xuyên)</div>
        <div style='padding: 10px; background: #fff; border-radius: 8px; border: 1px solid #e2e8f0;'>
            {scoreHtml}
        </div>
    </div>

    <!-- Section 3: Điểm thi / Tổng kết -->
    {examGradesHtml}

    <!-- Section 4: Học liệu -->
    {materialsHtml}

    <div class='text-center mt-3'>
        <a href='/Parent/Lessons?ChildId={studentId}&LessonId={lessonId}' class='btn btn-sm px-4 fw-semibold text-white d-inline-block' style='background: linear-gradient(135deg, #4F46E5, #7C3AED); border: none; border-radius: 8px; font-size: 0.82rem; text-decoration: none; padding: 8px 18px; box-shadow: 0 4px 12px rgba(79, 70, 229, 0.25);'>
            <i class='bi bi-folder2-open me-1'></i> Xem chi tiết buổi học
        </a>
    </div>
</div>";

                var notification = new Notification
                {
                    ParentId = parentId,
                    ClassId = classId,
                    Title = isRebroadcast ? $"Cập nhật học tập & chuyên cần - {studentName}" : $"Báo cáo học tập & chuyên cần - {studentName}",
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

        public async Task NotifyDailyGradeUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string lessonTitle,
            string studentName,
            decimal? score,
            string? comment)
        {
            var title = $"Cập nhật điểm Thường xuyên Buổi {lessonTitle} - {studentName}";
            var scoreText = score.HasValue ? $"<strong class='text-primary'>{score.Value.ToString("0.##")}</strong> / 10" : "Chưa nhập";
            var commentText = !string.IsNullOrWhiteSpace(comment) ? $"<div class='mt-1 text-muted small'>Nhận xét: {comment}</div>" : "";

            var messageBody = $@"
<div class='published-daily-grade' style='font-family: inherit; color: #1e293b; background: #fff;'>
    <div style='background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); border-radius: 8px; padding: 12px; margin-bottom: 16px;'>
        <div style='margin-bottom: 4px;'><b>Lớp học:</b> {className}</div>
        <div style='margin-bottom: 4px;'><b>Buổi học:</b> {lessonTitle}</div>
    </div>
    <div style='margin-bottom: 16px;'>
        <div style='font-weight: 700; font-size: 0.78rem; text-transform: uppercase; color: #64748b; margin-bottom: 6px; letter-spacing: 0.05em;'>📝 Kết quả bài học (Thường xuyên)</div>
        <div style='padding: 10px; background: #fff; border-radius: 8px; border: 1px solid #e2e8f0;'>
            <div><b>Điểm số:</b> {scoreText} {commentText}</div>
        </div>
    </div>
</div>";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = title,
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

        public async Task NotifyClassTranscriptUpdatedAsync(
            int parentId,
            int classId,
            string className,
            string studentName,
            decimal? midtermScore,
            string? midtermComment,
            decimal? finalScore,
            string? finalComment,
            decimal? averageDailyScore,
            decimal? finalScoreTotal)
        {
            var title = $"Trung tâm đã cập nhật bảng điểm định kỳ (Giữa kỳ/Cuối kỳ) - {studentName}";
            
            var tbText = averageDailyScore.HasValue ? $"<strong>{averageDailyScore.Value.ToString("0.##")}</strong> / 10" : "—";
            var gkText = midtermScore.HasValue ? $"<strong>{midtermScore.Value.ToString("0.##")}</strong> / 10" : "—";
            var ckText = finalScore.HasValue ? $"<strong>{finalScore.Value.ToString("0.##")}</strong> / 10" : "—";
            var totalText = finalScoreTotal.HasValue ? $"<span class='badge bg-success'>{finalScoreTotal.Value.ToString("0.##")}</span>" : "—";

            var gkCommentText = !string.IsNullOrWhiteSpace(midtermComment) ? $"<div class='text-muted small mt-1'>Nhận xét GK: {midtermComment}</div>" : "";
            var ckCommentText = !string.IsNullOrWhiteSpace(finalComment) ? $"<div class='text-muted small mt-1'>Nhận xét CK: {finalComment}</div>" : "";

            var messageBody = $@"
<div class='published-transcript-report' style='font-family: inherit; color: #1e293b; background: #fff;'>
    <div style='background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); border-radius: 8px; padding: 12px; margin-bottom: 16px;'>
        <div style='margin-bottom: 4px;'><b>Lớp học:</b> {className}</div>
        <div style='margin-bottom: 4px;'><b>Học sinh:</b> {studentName}</div>
    </div>
    <div style='margin-bottom: 16px;'>
        <div style='font-weight: 700; font-size: 0.78rem; text-transform: uppercase; color: #64748b; margin-bottom: 6px; letter-spacing: 0.05em;'>🏆 Đánh giá & Điểm tổng hợp</div>
        <div style='padding: 12px; background: #fff; border-radius: 8px; border: 1px solid #e2e8f0; font-size: 0.85rem;'>
            <div style='margin-bottom: 6px;'><b>Trung bình Thường xuyên (30%):</b> {tbText}</div>
            <div style='margin-bottom: 6px;'><b>Điểm Giữa kỳ (30%):</b> {gkText} {gkCommentText}</div>
            <div style='margin-bottom: 6px;'><b>Điểm Cuối kỳ (40%):</b> {ckText} {ckCommentText}</div>
            <hr style='opacity: 0.15; margin: 8px 0;'>
            <div style='font-weight: 700; font-size: 0.9rem;'>Tổng kết lớp học: {totalText}</div>
        </div>
    </div>
</div>";

            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = title,
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

        public async Task NotifyTeacherAssignedClassAsync(int teacherId, int classId, string className)
        {
            if (teacherId <= 0) return;

            var messageBody = $@"
<div class='mb-2'><b>Lớp học:</b> <span class='badge bg-indigo text-white' style='background-color: #4F46E5;'>{className}</span></div>
<hr style='opacity: 0.15; margin: 10px 0;'>
<div class='text-muted small' style='font-size: 0.85rem;'>Bạn đã được phân công làm giáo viên giảng dạy cho lớp học này. Vui lòng kiểm tra lịch học tại trang quản lý lớp học của tôi.</div>
";

            var notification = new Notification
            {
                ParentId = teacherId, // Dùng chung trường ParentId làm UserId người nhận
                ClassId = classId,
                Title = $"Phân công giảng dạy lớp {className}",
                Message = messageBody,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            var unreadCount = await _notificationRepository.CountUnreadByParentAsync(teacherId);

            var dto = new NotificationResponseDto
            {
                Id = notification.Id,
                ClassId = notification.ClassId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _realtimeNotifier.PushNotificationToParentAsync(teacherId, dto, unreadCount);
        }
    }
}

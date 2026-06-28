using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;
using PROJECT_PRN232_.Services.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public NotificationService(
            INotificationRepository notificationRepository,
            IRealtimeNotifier realtimeNotifier)
        {
            _notificationRepository = notificationRepository;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task NotifyScoreUpdatedAsync(
            int parentId, int classId, string className, string lessonTitle, string studentName, decimal? score)
        {
            var scoreText = score.HasValue ? score.Value.ToString("0.##") : "Chưa có";
            var notification = new Notification
            {
                ParentId = parentId,
                ClassId = classId,
                Title = $"Cập nhật điểm - {studentName}",
                Message = $"Lớp {className}, buổi \"{lessonTitle}\": {studentName} được chấm điểm {scoreText}/10.",
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

        public async Task MarkAllAsReadByParentAsync(int parentId)
        {
            await _notificationRepository.MarkAllAsReadByParentAsync(parentId);
        }
    }
}

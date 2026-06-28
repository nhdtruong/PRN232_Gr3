using PROJECT_PRN232_.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services
{
    public interface INotificationService
    {
        Task NotifyScoreUpdatedAsync(int parentId, int classId, string className, string lessonTitle, string studentName, decimal? score);
        Task<List<NotificationResponseDto>> GetLatestNotificationsByParentAsync(int parentId, int limit);
        Task MarkAllAsReadByParentAsync(int parentId);
    }
}

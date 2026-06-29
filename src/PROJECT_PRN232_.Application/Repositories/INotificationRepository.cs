using PROJECT_PRN232_.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<int> CountUnreadByParentAsync(int parentId);
        Task<List<Notification>> GetLatestNotificationsByParentAsync(int parentId, int limit);
        Task MarkAllAsReadByParentAsync(int parentId);
        /// <summary>Đánh dấu 1 thông báo là đã đọc. Trả về false nếu không tìm thấy hoặc không có quyền.</summary>
        Task<bool> MarkSingleAsReadAsync(int notificationId, int parentId);
    }
}

using PROJECT_PRN232_.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<int> CountUnreadByParentAsync(int parentId);
        Task<List<Notification>> GetLatestNotificationsByParentAsync(int parentId, int limit);
        Task MarkAllAsReadByParentAsync(int parentId);
    }
}

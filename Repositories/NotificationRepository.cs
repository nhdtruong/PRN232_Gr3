using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<int> CountUnreadByParentAsync(int parentId)
        {
            return await _context.Notifications
                .CountAsync(n => n.ParentId == parentId && !n.IsRead);
        }

        public async Task<List<Notification>> GetLatestNotificationsByParentAsync(int parentId, int limit)
        {
            return await _context.Notifications
                .Where(n => n.ParentId == parentId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkAllAsReadByParentAsync(int parentId)
        {
            var unread = await _context.Notifications
                .Where(n => n.ParentId == parentId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}

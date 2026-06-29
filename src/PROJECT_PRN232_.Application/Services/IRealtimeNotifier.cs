using PROJECT_PRN232_.Application.DTOs;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IRealtimeNotifier
    {
        Task PushNotificationToParentAsync(int parentId, NotificationResponseDto notification, int newUnreadCount);
    }
}

using PROJECT_PRN232_.DTOs;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services.Realtime
{
    public interface IRealtimeNotifier
    {
        Task PushNotificationToParentAsync(int parentId, NotificationResponseDto notification, int newUnreadCount);
    }
}

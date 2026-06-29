using Microsoft.AspNetCore.SignalR;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Api.Hubs;
using PROJECT_PRN232_.Application.Services;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Api
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRRealtimeNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushNotificationToParentAsync(int parentId, NotificationResponseDto notification, int newUnreadCount)
        {
            await _hubContext.Clients.User(parentId.ToString())
                .SendAsync("ReceiveNotification", notification, newUnreadCount);
        }
    }
}

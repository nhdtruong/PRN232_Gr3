using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PROJECT_PRN232_.Application.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationHub(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Parent" && int.TryParse(userIdString, out int parentId))
            {
                int unreadCount = await _notificationRepository.CountUnreadByParentAsync(parentId);
                await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
            }

            await base.OnConnectedAsync();
        }
    }
}

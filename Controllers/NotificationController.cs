using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize(Roles = "Parent,Center", AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest([FromQuery] int limit = 20)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính phụ huynh." });
            }

            var notifications = await _notificationService.GetLatestNotificationsByParentAsync(parentId, limit);
            return Ok(notifications);
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính phụ huynh." });
            }

            await _notificationService.MarkAllAsReadByParentAsync(parentId);
            return Ok(new { message = "Đã đánh dấu tất cả thông báo là đã đọc.", unreadCount = 0 });
        }

        /// <summary>Đánh dấu 1 thông báo cụ thể là đã đọc. Trả về unreadCount mới.</summary>
        [HttpPut("{id:int}/mark-read")]
        public async Task<IActionResult> MarkSingleRead(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính." });
            }

            var newUnreadCount = await _notificationService.MarkSingleAsReadAsync(id, parentId);
            return Ok(new { unreadCount = newUnreadCount });
        }
    }
}

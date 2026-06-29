using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IWebHostEnvironment _env;

        public ChatController(IChatService chatService, IWebHostEnvironment env)
        {
            _chatService = chatService;
            _env = env;
        }

        // GET /api/chat/channels
        [HttpGet("channels")]
        public async Task<IActionResult> GetChannels()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            }

            var channels = await _chatService.GetUserChannelsAsync(userId, role);
            return Ok(channels);
        }

        // GET /api/chat/channels/{channelId}/messages
        [HttpGet("channels/{channelId}/messages")]
        public async Task<IActionResult> GetChannelMessages(int channelId, [FromQuery] int limit = 50)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            }

            var isMember = await _chatService.IsChannelMemberAsync(channelId, userId);
            if (!isMember)
            {
                return Forbid();
            }

            var messages = await _chatService.GetChannelMessagesAsync(channelId, userId, limit);
            return Ok(messages);
        }

        // POST /api/chat/upload
        // Upload file (ảnh, video, tài liệu) trước khi gửi qua SignalR.
        // Pattern: "Upload HTTP REST -> Nhận URL -> Gửi URL qua SignalR Hub"
        [HttpPost("upload")]
        [RequestSizeLimit(25_000_000)] // Giới hạn tổng request 25MB
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không có file nào được gửi lên. Vui lòng chọn file hợp lệ." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Xác định loại file và kiểm tra giới hạn dung lượng
            string subFolder;
            MessageType detectedType;
            long maxSizeBytes;

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
            var docExtensions   = new[] { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };

            if (Array.Exists(imageExtensions, e => e == extension))
            {
                detectedType = MessageType.Image;
                subFolder = "images";
                maxSizeBytes = 5 * 1024 * 1024; // 5MB
            }
            else if (Array.Exists(videoExtensions, e => e == extension))
            {
                detectedType = MessageType.Video;
                subFolder = "videos";
                maxSizeBytes = 20 * 1024 * 1024; // 20MB
            }
            else if (Array.Exists(docExtensions, e => e == extension))
            {
                detectedType = MessageType.Document;
                subFolder = "docs";
                maxSizeBytes = 10 * 1024 * 1024; // 10MB
            }
            else
            {
                return BadRequest(new
                {
                    message = $"Định dạng file '{extension}' không được hỗ trợ. Chỉ chấp nhận: Ảnh (JPG/PNG/GIF/WEBP), Video (MP4/MOV/AVI/MKV), Tài liệu (PDF/DOCX/XLSX)."
                });
            }

            // Kiểm tra dung lượng
            if (file.Length > maxSizeBytes)
            {
                var limitMb = maxSizeBytes / (1024 * 1024);
                return BadRequest(new
                {
                    message = $"File vượt quá giới hạn dung lượng cho phép. Giới hạn: {limitMb}MB. Dung lượng file: {file.Length / 1024 / 1024.0:F1}MB."
                });
            }

            // Tạo thư mục lưu trữ nếu chưa tồn tại
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Đặt tên file mới bằng GUID để tránh trùng lặp
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            // Lưu file vào đĩa
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về URL truy cập công khai
            var fileUrl = $"/uploads/{subFolder}/{uniqueFileName}";

            return Ok(new
            {
                fileUrl = fileUrl,
                messageType = (int)detectedType,
                originalName = Path.GetFileName(file.FileName),
                sizeBytes = file.Length
            });
        }
    }
}

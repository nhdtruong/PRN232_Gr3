using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Gia nhập vào group SignalR của Channel để nhận tin nhắn real-time
        public async Task JoinChannel(int channelId)
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                var isMember = await _chatService.IsChannelMemberAsync(channelId, userId);
                if (isMember)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, channelId.ToString());
                }
            }
        }

        // Gửi tin nhắn văn bản qua Hub
        // Tham số messageType: 0=Text, 1=Image, 2=Video, 3=Document
        // Tham số fileUrl: URL file (null nếu là Text)
        // Tham số messageContent: nội dung văn bản hoặc tên file gốc
        public async Task SendMessage(int channelId, string messageContent, int messageType = 0, string? fileUrl = null)
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int senderId)) return;

            // Validate: tin nhắn Text phải có nội dung
            var typedMessageType = (MessageType)messageType;
            if (typedMessageType == MessageType.Text && string.IsNullOrWhiteSpace(messageContent)) return;

            // Validate: tin nhắn file phải có URL
            if (typedMessageType != MessageType.Text && string.IsNullOrWhiteSpace(fileUrl)) return;

            var isMember = await _chatService.IsChannelMemberAsync(channelId, senderId);
            if (!isMember) return;

            // Lưu vào Database
            var chatMessageDto = await _chatService.SendMessageAsync(
                channelId,
                senderId,
                messageContent,
                typedMessageType,
                fileUrl
            );

            var channel = await _chatService.GetChannelByIdAsync(channelId);
            if (channel == null) return;

            // Broadcast vào group Channel (cả 2 bên đang mở chat đều nhận được)
            await Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", chatMessageDto);

            // Push trực tiếp đến người nhận qua UserID (để cập nhật badge và Toast khi đang ở trang khác)
            var recipientId = (channel.CenterId == senderId) ? channel.ParentId : channel.CenterId;
            await Clients.User(recipientId.ToString()).SendAsync("ReceiveChatNotification", chatMessageDto);
        }
    }
}

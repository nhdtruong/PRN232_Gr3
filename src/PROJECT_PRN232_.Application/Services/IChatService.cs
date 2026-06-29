using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IChatService
    {
        Task<List<ChatChannelResponseDto>> GetUserChannelsAsync(int userId, string role);
        Task<List<ChatMessageResponseDto>> GetChannelMessagesAsync(int channelId, int userId, int limit = 50);
        Task<ChatMessageResponseDto> SendMessageAsync(int channelId, int senderId, string messageContent, MessageType messageType = MessageType.Text, string? fileUrl = null);
        Task<ChatChannelResponseDto> GetOrCreateChannelAsync(int centerId, int parentId);
        Task<bool> IsChannelMemberAsync(int channelId, int userId);
        Task<ChatChannelResponseDto?> GetChannelByIdAsync(int channelId);
        Task<int> GetUnreadChatCountAsync(int userId);
    }
}

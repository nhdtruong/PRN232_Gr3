using PROJECT_PRN232_.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Repositories
{
    public interface IChatRepository
    {
        Task<List<ChatChannel>> GetChannelsByUserIdAsync(int userId, string role);
        Task<ChatChannel?> GetChannelByIdAsync(int channelId);
        Task<ChatChannel?> GetChannelByMembersAsync(int centerId, int parentId);
        Task<List<ChatMessage>> GetMessagesByChannelIdAsync(int channelId, int limit = 50);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<ChatChannel> CreateChannelAsync(ChatChannel channel);
        Task MarkMessagesAsReadAsync(int channelId, int readerId);
        Task<int> GetUnreadChatCountAsync(int userId);
    }
}

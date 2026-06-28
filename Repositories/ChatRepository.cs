using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatChannel>> GetChannelsByUserIdAsync(int userId, string role)
        {
            IQueryable<ChatChannel> query = _context.ChatChannels
                .Include(c => c.Center)
                .Include(c => c.Parent)
                .Include(c => c.ChatMessages);

            if (role == "Center")
            {
                query = query.Where(c => c.CenterId == userId);
            }
            else if (role == "Parent")
            {
                query = query.Where(c => c.ParentId == userId);
            }
            else
            {
                query = query.Where(c => c.CenterId == userId || c.ParentId == userId);
            }

            return await query.ToListAsync();
        }

        public async Task<ChatChannel?> GetChannelByIdAsync(int channelId)
        {
            return await _context.ChatChannels
                .Include(c => c.Center)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == channelId);
        }

        public async Task<ChatChannel?> GetChannelByMembersAsync(int centerId, int parentId)
        {
            return await _context.ChatChannels
                .Include(c => c.Center)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.CenterId == centerId && c.ParentId == parentId);
        }

        public async Task<List<ChatMessage>> GetMessagesByChannelIdAsync(int channelId, int limit = 50)
        {
            var rawMessages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .ToListAsync();

            // Reverse to render chronologically in the chat box
            return rawMessages.OrderBy(m => m.SentAt).ToList();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            
            // Reload with sender object populated
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();
            
            return message;
        }

        public async Task<ChatChannel> CreateChannelAsync(ChatChannel channel)
        {
            await _context.ChatChannels.AddAsync(channel);
            await _context.SaveChangesAsync();
            return channel;
        }

        public async Task MarkMessagesAsReadAsync(int channelId, int readerId)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.ChannelId == channelId && m.SenderId != readerId && !m.IsRead)
                .ToListAsync();

            if (unread.Count > 0)
            {
                foreach (var msg in unread)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadChatCountAsync(int userId)
        {
            return await _context.ChatMessages
                .Include(m => m.ChatChannel)
                .CountAsync(m => !m.IsRead && m.SenderId != userId && 
                    (m.ChatChannel.CenterId == userId || m.ChatChannel.ParentId == userId));
        }
    }
}

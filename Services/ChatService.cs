using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Data.Enums;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<List<ChatChannelResponseDto>> GetUserChannelsAsync(int userId, string role)
        {
            var channels = await _chatRepository.GetChannelsByUserIdAsync(userId, role);
            var dtos = new List<ChatChannelResponseDto>();

            foreach (var c in channels)
            {
                var lastMsg = c.ChatMessages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var otherUserName = role == "Center" ? c.Parent.FullName : c.Center.FullName;

                dtos.Add(new ChatChannelResponseDto
                {
                    Id = c.Id,
                    CenterId = c.CenterId,
                    CenterName = c.Center.FullName,
                    ParentId = c.ParentId,
                    ParentName = c.Parent.FullName,
                    OtherUserName = otherUserName,
                    LastMessage = lastMsg?.MessageContent,
                    LastMessageSentAt = lastMsg?.SentAt,
                    IsLastMessageRead = lastMsg?.IsRead ?? true,
                    LastMessageSenderId = lastMsg?.SenderId
                });
            }

            // Order by last message time, descending
            return dtos.OrderByDescending(d => d.LastMessageSentAt ?? DateTime.MinValue).ToList();
        }

        public async Task<List<ChatMessageResponseDto>> GetChannelMessagesAsync(int channelId, int userId, int limit = 50)
        {
            // Mark all messages from the other sender in this channel as read
            await _chatRepository.MarkMessagesAsReadAsync(channelId, userId);

            var messages = await _chatRepository.GetMessagesByChannelIdAsync(channelId, limit);
            return messages.Select(m => new ChatMessageResponseDto
            {
                Id = m.Id,
                ChannelId = m.ChannelId,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                MessageContent = m.MessageContent,
                MessageType = m.MessageType,
                FileUrl = m.FileUrl,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            }).ToList();
        }

        public async Task<ChatMessageResponseDto> SendMessageAsync(int channelId, int senderId, string messageContent, MessageType messageType = MessageType.Text, string? fileUrl = null)
        {
            var message = new ChatMessage
            {
                ChannelId = channelId,
                SenderId = senderId,
                MessageContent = messageContent,
                MessageType = messageType,
                FileUrl = fileUrl,
                IsRead = false,
                SentAt = DateTime.Now
            };

            var saved = await _chatRepository.AddMessageAsync(message);
            return new ChatMessageResponseDto
            {
                Id = saved.Id,
                ChannelId = saved.ChannelId,
                SenderId = saved.SenderId,
                SenderName = saved.Sender.FullName,
                MessageContent = saved.MessageContent,
                MessageType = saved.MessageType,
                FileUrl = saved.FileUrl,
                IsRead = saved.IsRead,
                SentAt = saved.SentAt
            };
        }

        public async Task<ChatChannelResponseDto> GetOrCreateChannelAsync(int centerId, int parentId)
        {
            var existing = await _chatRepository.GetChannelByMembersAsync(centerId, parentId);
            if (existing != null)
            {
                var lastMsg = existing.ChatMessages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                return new ChatChannelResponseDto
                {
                    Id = existing.Id,
                    CenterId = existing.CenterId,
                    CenterName = existing.Center.FullName,
                    ParentId = existing.ParentId,
                    ParentName = existing.Parent.FullName,
                    OtherUserName = existing.Parent.FullName, // Assuming center called it
                    LastMessage = lastMsg?.MessageContent,
                    LastMessageSentAt = lastMsg?.SentAt,
                    IsLastMessageRead = lastMsg?.IsRead ?? true,
                    LastMessageSenderId = lastMsg?.SenderId
                };
            }

            var channel = new ChatChannel
            {
                CenterId = centerId,
                ParentId = parentId,
                CreatedAt = DateTime.Now
            };

            var created = await _chatRepository.CreateChannelAsync(channel);
            // Reload channel with navigations loaded
            var fullyLoaded = await _chatRepository.GetChannelByIdAsync(created.Id);

            return new ChatChannelResponseDto
            {
                Id = fullyLoaded!.Id,
                CenterId = fullyLoaded.CenterId,
                CenterName = fullyLoaded.Center.FullName,
                ParentId = fullyLoaded.ParentId,
                ParentName = fullyLoaded.Parent.FullName,
                OtherUserName = fullyLoaded.Parent.FullName,
                LastMessage = null,
                LastMessageSentAt = null,
                IsLastMessageRead = true,
                LastMessageSenderId = null
            };
        }

        public async Task<bool> IsChannelMemberAsync(int channelId, int userId)
        {
            var channel = await _chatRepository.GetChannelByIdAsync(channelId);
            if (channel == null) return false;
            return channel.CenterId == userId || channel.ParentId == userId;
        }

        public async Task<ChatChannelResponseDto?> GetChannelByIdAsync(int channelId)
        {
            var channel = await _chatRepository.GetChannelByIdAsync(channelId);
            if (channel == null) return null;

            var lastMsg = channel.ChatMessages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            return new ChatChannelResponseDto
            {
                Id = channel.Id,
                CenterId = channel.CenterId,
                CenterName = channel.Center.FullName,
                ParentId = channel.ParentId,
                ParentName = channel.Parent.FullName,
                OtherUserName = "", // Not context specific here
                LastMessage = lastMsg?.MessageContent,
                LastMessageSentAt = lastMsg?.SentAt,
                IsLastMessageRead = lastMsg?.IsRead ?? true,
                LastMessageSenderId = lastMsg?.SenderId
            };
        }

        public async Task<int> GetUnreadChatCountAsync(int userId)
        {
            return await _chatRepository.GetUnreadChatCountAsync(userId);
        }
    }
}

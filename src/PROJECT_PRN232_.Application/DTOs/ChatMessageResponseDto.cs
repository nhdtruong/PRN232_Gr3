using PROJECT_PRN232_.Domain;
using System;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ChatMessageResponseDto
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        // Loại tin nhắn: Text=0, Image=1, Video=2, Document=3
        public MessageType MessageType { get; set; } = MessageType.Text;
        // URL file nếu là Image/Video/Document
        public string? FileUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}

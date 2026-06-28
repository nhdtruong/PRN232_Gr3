using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PROJECT_PRN232_.Data.Enums;

namespace PROJECT_PRN232_.Data.Entities
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChannelId { get; set; }

        [ForeignKey(nameof(ChannelId))]
        public virtual ChatChannel ChatChannel { get; set; } = null!;

        [Required]
        public int SenderId { get; set; }
        public virtual User Sender { get; set; } = null!;

        // Nội dung văn bản, hoặc tên file nếu là file
        public string MessageContent { get; set; } = string.Empty;

        // Loại tin nhắn: Text=0, Image=1, Video=2, Document=3
        public MessageType MessageType { get; set; } = MessageType.Text;

        // URL file nếu là Image/Video/Document (null nếu là Text)
        public string? FileUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
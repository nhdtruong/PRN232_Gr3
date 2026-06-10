using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        public string MessageContent { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
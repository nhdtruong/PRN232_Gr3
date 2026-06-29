using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Domain
{
    public class ChatChannel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CenterId { get; set; }
        public virtual User Center { get; set; } = null!;

        [Required]
        public int ParentId { get; set; }
        public virtual User Parent { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
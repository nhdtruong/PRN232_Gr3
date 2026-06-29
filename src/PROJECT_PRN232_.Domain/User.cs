using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = null!;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; // 'Center' ho?c 'Parent'

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Class> CreatedClasses { get; set; } = new List<Class>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<ChatChannel> CenterChannels { get; set; } = new List<ChatChannel>();
        public virtual ICollection<ChatChannel> ParentChannels { get; set; } = new List<ChatChannel>();
        public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    }
}
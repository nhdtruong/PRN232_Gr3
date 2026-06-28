using System;

namespace PROJECT_PRN232_.DTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int? ClassId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

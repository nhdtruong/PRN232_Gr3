using System;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ChatChannelResponseDto
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public string CenterName { get; set; } = null!;
        public int ParentId { get; set; }
        public string ParentName { get; set; } = null!;
        public string OtherUserName { get; set; } = null!;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageSentAt { get; set; }
        public bool IsLastMessageRead { get; set; }
        public int? LastMessageSenderId { get; set; }
    }
}

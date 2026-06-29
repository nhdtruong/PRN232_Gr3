using PROJECT_PRN232_.Domain;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ChatMessageCreateDto
    {
        // Nội dung văn bản. Bắt buộc nếu MessageType == Text, có thể rỗng nếu là file.
        public string MessageContent { get; set; } = string.Empty;

        // Loại tin nhắn gửi lên: Text=0, Image=1, Video=2, Document=3
        public MessageType MessageType { get; set; } = MessageType.Text;

        // URL file sau khi upload xong qua /api/chat/upload (null nếu là Text)
        public string? FileUrl { get; set; }
    }
}

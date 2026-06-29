using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

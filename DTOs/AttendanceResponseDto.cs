using PROJECT_PRN232_.Data.Enums;

namespace PROJECT_PRN232_.DTOs
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

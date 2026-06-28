using PROJECT_PRN232_.Data.Enums;

namespace PROJECT_PRN232_.DTOs
{
    public class LessonRollCallRowDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ParentId { get; set; }

        public int? AttendanceId { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Note { get; set; }

        public int? AssessmentId { get; set; }
        public decimal? Score { get; set; }
        public string? TeacherComment { get; set; }
    }
}

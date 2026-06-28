using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class LessonAttendanceBulkDto
    {
        [Required]
        public List<AttendanceUpsertDto> Items { get; set; } = new();
    }
}

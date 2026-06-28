using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class LessonRollCallBulkUpsertDto
    {
        [Required]
        public List<LessonRollCallRowDto> Rows { get; set; } = new();
    }
}

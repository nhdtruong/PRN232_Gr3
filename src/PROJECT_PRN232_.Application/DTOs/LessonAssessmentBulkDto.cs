using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class LessonAssessmentBulkDto
    {
        [Required]
        public List<AssessmentUpsertDto> Items { get; set; } = new();
    }
}

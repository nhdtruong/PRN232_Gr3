using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class AssessmentUpsertDto
    {
        public int? Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm số phải nằm trong khoảng 0 - 10")]
        public decimal? Score { get; set; }

        public string? TeacherComment { get; set; }
    }
}

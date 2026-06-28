namespace PROJECT_PRN232_.DTOs
{
    public class AssessmentResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public decimal? Score { get; set; }
        public string? TeacherComment { get; set; }
        public DateTime DateAssessed { get; set; }
    }
}

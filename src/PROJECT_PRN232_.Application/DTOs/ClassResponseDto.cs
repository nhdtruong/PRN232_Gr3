namespace PROJECT_PRN232_.Application.DTOs
{
    public class ClassResponseDto
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CenterId { get; set; }
        public string Status { get; set; } = "Active";
        public int StudentCount { get; set; }
        public int MaxCapacity { get; set; }
        public string? Subject { get; set; }
        public int TotalLessons { get; set; } = 24;
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
    }
}

namespace PROJECT_PRN232_.Application.DTOs
{
    public class LessonRollCallResponseDto
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public List<LessonRollCallRowDto> Rows { get; set; } = new();
    }
}

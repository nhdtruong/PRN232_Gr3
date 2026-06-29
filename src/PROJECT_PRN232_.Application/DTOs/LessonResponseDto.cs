using System;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class LessonResponseDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime LessonDate { get; set; }
        public bool IsPublished { get; set; }
    }
}

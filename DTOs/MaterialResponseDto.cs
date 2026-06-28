using System;

namespace PROJECT_PRN232_.DTOs
{
    public class MaterialResponseDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty; // Document, Video, Homework
        public string FileURL { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}

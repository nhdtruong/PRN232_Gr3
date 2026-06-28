using System;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class LessonUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime LessonDate { get; set; }
    }
}

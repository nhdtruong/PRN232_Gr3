using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PROJECT_PRN232_.Data.Enums;

namespace PROJECT_PRN232_.Data.Entities
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; } = null!;

        [Required]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson Lesson { get; set; } = null!;

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [StringLength(255)]
        public string? Note { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

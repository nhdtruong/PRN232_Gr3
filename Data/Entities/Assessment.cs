using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Data.Entities
{
    public class Assessment
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

        public decimal? Score { get; set; } // Sẽ được cấu hình thành decimal(4,2) ở DbContext

        public string? TeacherComment { get; set; }

        public DateTime DateAssessed { get; set; } = DateTime.Now;
    }
}
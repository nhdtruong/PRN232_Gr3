using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        // Gắn với Buổi học (giữ nguyên để tương thích với code cũ)
        public int? LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson? Lesson { get; set; }

        // Gắn với Môn học (Subject) — mới thêm cho Người 4
        public int? SubjectId { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject? Subject { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = null!; // 'Document', 'Video', 'Homework'

        [Required]
        [StringLength(2000)]
        public string FileURL { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
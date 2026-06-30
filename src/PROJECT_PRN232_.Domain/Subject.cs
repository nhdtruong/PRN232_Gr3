using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CenterId { get; set; }

        [ForeignKey(nameof(CenterId))]
        public virtual User Center { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string SubjectCode { get; set; } = null!; // Ví dụ: ENG101, MATH_BASIC

        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; } = null!; // Tên môn học

        public string? Description { get; set; }

        [Required]
        public int NumberOfSessions { get; set; } = 1; // Số buổi học của môn

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}

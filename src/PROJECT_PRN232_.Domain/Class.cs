using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CenterId { get; set; }

        [ForeignKey(nameof(CenterId))]
        public virtual User Center { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string ClassName { get; set; } = null!;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public int MaxCapacity { get; set; } = 30;

        [StringLength(100)]
        public string? Subject { get; set; }

        public int TotalLessons { get; set; } = 24;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
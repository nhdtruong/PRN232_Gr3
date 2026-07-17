using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual User Parent { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<DailyAssessment> DailyAssessments { get; set; } = new List<DailyAssessment>();
        public virtual ICollection<ClassTranscript> ClassTranscripts { get; set; } = new List<ClassTranscript>();
    }
}
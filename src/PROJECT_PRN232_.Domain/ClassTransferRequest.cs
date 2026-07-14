using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class ClassTransferRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; } = null!;

        [Required]
        public int FromTeacherId { get; set; }

        [ForeignKey(nameof(FromTeacherId))]
        public virtual User FromTeacher { get; set; } = null!;

        [Required]
        public int ToTeacherId { get; set; }

        [ForeignKey(nameof(ToTeacherId))]
        public virtual User ToTeacher { get; set; } = null!;

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}

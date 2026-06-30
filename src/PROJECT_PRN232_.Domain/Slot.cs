using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Domain
{
    public class Slot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SlotName { get; set; } = null!;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Navigation property
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

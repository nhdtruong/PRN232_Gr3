using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Domain
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RoomName { get; set; } = null!;

        public int Capacity { get; set; }

        [StringLength(255)]
        public string? Equipment { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Maintenance

        // Navigation property
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

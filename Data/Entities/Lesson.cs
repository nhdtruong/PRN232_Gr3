using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Data.Entities
{
	public class Lesson
	{
       [Key]
		public int Id { get; set; }

		[Required]
		public int ClassId { get; set; }

		[ForeignKey(nameof(ClassId))]
		public virtual Class Class { get; set; } = null!;

		[Required]
		[StringLength(255)]
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		[Required]
		public DateTime LessonDate { get; set; }

		public bool IsPublished { get; set; } = false;

		// Navigation Properties
		public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
		public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
		public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
	}
}
using System;

namespace PROJECT_PRN232_.Data.Entities
{
    public class ClassStudent
    {
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.Now;
    }
}
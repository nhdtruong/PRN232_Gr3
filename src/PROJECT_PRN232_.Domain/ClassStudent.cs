using System;

namespace PROJECT_PRN232_.Domain
{
    public class ClassStudent
    {
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        public decimal? MidtermScore { get; set; }
        public string? MidtermComment { get; set; }

        public decimal? FinalScore { get; set; }
        public string? FinalComment { get; set; }
    }
}
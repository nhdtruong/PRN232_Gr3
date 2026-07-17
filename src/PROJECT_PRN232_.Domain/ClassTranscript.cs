using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJECT_PRN232_.Domain
{
    public class ClassTranscript
    {
        [Required]
        public int ClassId { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; } = null!;

        public decimal? MidTermScore { get; set; }
        public string? MidTermComment { get; set; }

        public decimal? FinalScore { get; set; }
        public string? FinalComment { get; set; }

        public decimal? AverageDailyScore { get; set; }
        public decimal? FinalScoreTotal { get; set; }
    }
}

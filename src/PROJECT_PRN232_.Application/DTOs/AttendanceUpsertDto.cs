using System.ComponentModel.DataAnnotations;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class AttendanceUpsertDto
    {
        public int? Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [StringLength(255)]
        public string? Note { get; set; }
    }
}

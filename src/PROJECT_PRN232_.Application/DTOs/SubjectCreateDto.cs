using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class SubjectCreateDto
    {
        [Required(ErrorMessage = "Mã môn học không được để trống.")]
        [StringLength(50, ErrorMessage = "Mã môn học tối đa 50 ký tự.")]
        public string SubjectCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên môn học không được để trống.")]
        [StringLength(255, ErrorMessage = "Tên môn học tối đa 255 ký tự.")]
        public string SubjectName { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Số buổi học không được để trống.")]
        [Range(1, 500, ErrorMessage = "Số buổi học phải từ 1 đến 500.")]
        public int NumberOfSessions { get; set; } = 1;
    }
}

using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ClassUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên lớp học không được để trống")]
        [StringLength(100, ErrorMessage = "Tên lớp không được quá 100 ký tự")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã trung tâm không được để trống")]
        public int CenterId { get; set; }

        public int MaxCapacity { get; set; }

        public string? Subject { get; set; }

        public int TotalLessons { get; set; } = 24;

        public int? TeacherId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.DTOs
{
    public class MaterialCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = "Document"; // Document, Video, Homework

        [Required]
        [StringLength(2000)]
        public string FileURL { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace PROJECT_PRN232_.Data.Entities
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public int CenterId { get; set; }
    }
}

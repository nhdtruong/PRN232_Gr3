namespace PROJECT_PRN232_.DTOs
{
    public class ClassResponseDto
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CenterId { get; set; }
        public string Status { get; set; } = "Active";
        public int StudentCount { get; set; }
    }
}

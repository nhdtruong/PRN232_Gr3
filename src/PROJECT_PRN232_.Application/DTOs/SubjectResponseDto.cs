using System;
using System.Collections.Generic;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class SubjectResponseDto
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int NumberOfSessions { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MaterialCount { get; set; }
    }
}

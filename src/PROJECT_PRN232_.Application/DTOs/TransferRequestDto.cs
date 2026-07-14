using System;

namespace PROJECT_PRN232_.Application.DTOs
{
    public class ClassTransferRequestDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        
        public int FromTeacherId { get; set; }
        public string FromTeacherName { get; set; } = string.Empty;

        public int ToTeacherId { get; set; }
        public string ToTeacherName { get; set; } = string.Empty;

        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }

    public class TransferRequestCreateDto
    {
        public int ClassId { get; set; }
        public int FromTeacherId { get; set; }
        public int ToTeacherId { get; set; }
        public string? Reason { get; set; }
    }

    public class TransferRequestStatusUpdateDto
    {
        public bool IsApproved { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Controllers
{
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class ClassGradeSheetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ClassGradeSheetController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Lưu bảng điểm tổng hợp cho một lớp học.
        /// POST /api/classes/{classId}/gradesheet
        /// </summary>
        [HttpPost("api/classes/{classId}/gradesheet")]
        public async Task<IActionResult> SaveGradeSheet(int classId, [FromBody] List<GradeEntryDto> entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest(new { message = "Không có dữ liệu điểm để lưu." });

            // Lấy teacherId từ cookie claim
            var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(teacherIdStr, out int teacherId))
                return Unauthorized(new { message = "Không xác định được giáo viên." });

            // Xác minh giáo viên sở hữu lớp
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId);
            if (classEntity == null)
                return NotFound(new { message = $"Không tìm thấy lớp học ID={classId} hoặc bạn không có quyền truy cập." });

            // Danh sách lessonId hợp lệ của lớp
            var validLessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .ToListAsync();
            var validLessonIds = validLessons.Select(l => l.Id).ToHashSet();

            // Danh sách học sinh trong lớp
            var classStudents = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Student)
                .ToListAsync();
            var classStudentsMap = classStudents.ToDictionary(cs => cs.StudentId);

            var now = DateTime.Now;
            int saved = 0;

            foreach (var entry in entries)
            {
                if (!classStudentsMap.TryGetValue(entry.StudentId, out var classStudent))
                    continue;

                if (entry.Score.HasValue && (entry.Score < 0 || entry.Score > 10))
                    return BadRequest(new { message = $"Điểm phải từ 0-10 (StudentId={entry.StudentId})" });

                var scoreType = (entry.ScoreType ?? "TX").ToUpper();

                if (scoreType == "TX")
                {
                    // TX: Upsert vào table Assessments
                    if (!validLessonIds.Contains(entry.LessonId)) continue;

                    var assessment = await _context.Assessments
                        .FirstOrDefaultAsync(a => a.StudentId == entry.StudentId && a.LessonId == entry.LessonId);

                    bool isNew = false;
                    bool isChanged = false;

                    if (assessment == null)
                    {
                        if (!entry.Score.HasValue) continue;
                        assessment = new Assessment
                        {
                            StudentId = entry.StudentId,
                            LessonId = entry.LessonId,
                            Score = entry.Score,
                            TeacherComment = string.IsNullOrWhiteSpace(entry.TeacherComment) ? null : entry.TeacherComment.Trim(),
                            DateAssessed = now
                        };
                        _context.Assessments.Add(assessment);
                        isNew = true;
                        isChanged = true;
                    }
                    else
                    {
                        if (assessment.Score != entry.Score || assessment.TeacherComment != entry.TeacherComment)
                        {
                            assessment.Score = entry.Score;
                            assessment.TeacherComment = string.IsNullOrWhiteSpace(entry.TeacherComment) ? null : entry.TeacherComment.Trim();
                            assessment.DateAssessed = now;
                            isChanged = true;
                        }
                    }

                    if (isChanged)
                    {
                        saved++;
                    }
                }
                else if (scoreType == "GK")
                {
                    // Giữa kỳ: lưu trực tiếp vào ClassStudent
                    if (classStudent.MidtermScore != entry.Score || classStudent.MidtermComment != entry.TeacherComment)
                    {
                        classStudent.MidtermScore = entry.Score;
                        classStudent.MidtermComment = entry.TeacherComment;
                        saved++;
                    }
                }
                else if (scoreType == "CK")
                {
                    // Cuối kỳ: lưu trực tiếp vào ClassStudent
                    if (classStudent.FinalScore != entry.Score || classStudent.FinalComment != entry.TeacherComment)
                    {
                        classStudent.FinalScore = entry.Score;
                        classStudent.FinalComment = entry.TeacherComment;
                        saved++;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã lưu thành công {saved} mục điểm.",
                saved
            });
        }
    }

    public class GradeEntryDto
    {
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public int? AssessmentId { get; set; }
        public decimal? Score { get; set; }
        public string? TeacherComment { get; set; }
        /// <summary>TX | GK | CK</summary>
        public string? ScoreType { get; set; }
    }
}

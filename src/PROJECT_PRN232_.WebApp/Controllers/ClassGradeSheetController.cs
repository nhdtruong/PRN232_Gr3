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
        /// Lưu điểm thường xuyên (Daily assessment) của một buổi học cụ thể.
        /// POST /api/classes/{classId}/gradesheet
        /// </summary>
        [HttpPost("api/classes/{classId}/gradesheet")]
        public async Task<IActionResult> SaveGradeSheet(int classId, [FromBody] List<GradeEntryDto> entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest(new { message = "Không có dữ liệu điểm để lưu." });

            var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(teacherIdStr, out int teacherId))
                return Unauthorized(new { message = "Không xác định được giáo viên." });

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId);
            if (classEntity == null)
                return NotFound(new { message = $"Không tìm thấy lớp học ID={classId} hoặc bạn không có quyền truy cập." });

            var validLessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .ToListAsync();
            var validLessonIds = validLessons.Select(l => l.Id).ToHashSet();

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

                if (!validLessonIds.Contains(entry.LessonId)) continue;

                var assessment = await _context.DailyAssessments
                    .FirstOrDefaultAsync(a => a.StudentId == entry.StudentId && a.LessonId == entry.LessonId);

                bool isChanged = false;

                if (assessment == null)
                {
                    if (!entry.Score.HasValue) continue;
                    assessment = new DailyAssessment
                    {
                        StudentId = entry.StudentId,
                        LessonId = entry.LessonId,
                        Score = entry.Score,
                        Comment = string.IsNullOrWhiteSpace(entry.TeacherComment) ? null : entry.TeacherComment.Trim(),
                        DateAssessed = now
                    };
                    _context.DailyAssessments.Add(assessment);
                    isChanged = true;
                }
                else
                {
                    if (assessment.Score != entry.Score || assessment.Comment != entry.TeacherComment)
                    {
                        assessment.Score = entry.Score;
                        assessment.Comment = string.IsNullOrWhiteSpace(entry.TeacherComment) ? null : entry.TeacherComment.Trim();
                        assessment.DateAssessed = now;
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    saved++;
                    
                    // Gửi thông báo ngay lập tức cho Phụ huynh qua SignalR
                    if (classStudent.Student.ParentId > 0)
                    {
                        var lesson = validLessons.FirstOrDefault(l => l.Id == entry.LessonId);
                        var lessonTitle = lesson?.Title ?? $"Buổi {entry.LessonId}";

                        try
                        {
                            await _notificationService.NotifyDailyGradeUpdatedAsync(
                                classStudent.Student.ParentId,
                                classId,
                                classEntity.ClassName,
                                lessonTitle,
                                classStudent.Student.FullName,
                                entry.Score,
                                entry.TeacherComment);
                        }
                        catch (Exception ex)
                        {
                            // Ghi log lỗi gửi thông báo nhưng không làm gián đoạn việc lưu điểm
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã lưu thành công {saved} điểm thường xuyên.",
                saved
            });
        }

        /// <summary>
        /// Lưu bảng điểm tổng kết (Giữa kỳ, Cuối kỳ) cho lớp học.
        /// POST /api/classes/{classId}/transcript
        /// </summary>
        [HttpPost("api/classes/{classId}/transcript")]
        public async Task<IActionResult> SaveClassTranscript(int classId, [FromBody] List<TranscriptEntryDto> entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest(new { message = "Không có dữ liệu bảng điểm để lưu." });

            var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(teacherIdStr, out int teacherId))
                return Unauthorized(new { message = "Không xác định được giáo viên." });

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId);
            if (classEntity == null)
                return NotFound(new { message = $"Không tìm thấy lớp học ID={classId} hoặc bạn không có quyền truy cập." });

            var classStudents = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Student)
                .ToListAsync();
            var classStudentsMap = classStudents.ToDictionary(cs => cs.StudentId);

            int saved = 0;

            foreach (var entry in entries)
            {
                if (!classStudentsMap.TryGetValue(entry.StudentId, out var classStudent))
                    continue;

                if (entry.QuizScore.HasValue && (entry.QuizScore < 0 || entry.QuizScore > 10))
                    return BadRequest(new { message = $"Điểm Quiz phải từ 0-10 (StudentId={entry.StudentId})" });

                if (entry.MidTermScore.HasValue && (entry.MidTermScore < 0 || entry.MidTermScore > 10))
                    return BadRequest(new { message = $"Điểm giữa kỳ phải từ 0-10 (StudentId={entry.StudentId})" });

                if (entry.FinalScore.HasValue && (entry.FinalScore < 0 || entry.FinalScore > 10))
                    return BadRequest(new { message = $"Điểm cuối kỳ phải từ 0-10 (StudentId={entry.StudentId})" });

                // Tính toán FinalScoreTotal nếu đủ 3 đầu điểm (Quiz 30%, Giữa kỳ 30%, Cuối kỳ 40%)
                decimal? finalScoreTotal = null;
                if (entry.QuizScore.HasValue && entry.MidTermScore.HasValue && entry.FinalScore.HasValue)
                {
                    finalScoreTotal = Math.Round(entry.QuizScore.Value * 0.3m + entry.MidTermScore.Value * 0.3m + entry.FinalScore.Value * 0.4m, 2);
                }

                var transcript = await _context.ClassTranscripts
                    .FirstOrDefaultAsync(ct => ct.ClassId == classId && ct.StudentId == entry.StudentId);

                bool isChanged = false;

                if (transcript == null)
                {
                    transcript = new ClassTranscript
                    {
                        ClassId = classId,
                        StudentId = entry.StudentId,
                        QuizScore = entry.QuizScore,
                        QuizComment = string.IsNullOrWhiteSpace(entry.QuizComment) ? null : entry.QuizComment.Trim(),
                        MidTermScore = entry.MidTermScore,
                        MidTermComment = string.IsNullOrWhiteSpace(entry.MidTermComment) ? null : entry.MidTermComment.Trim(),
                        FinalScore = entry.FinalScore,
                        FinalComment = string.IsNullOrWhiteSpace(entry.FinalComment) ? null : entry.FinalComment.Trim(),
                        FinalScoreTotal = finalScoreTotal
                    };
                    _context.ClassTranscripts.Add(transcript);
                    isChanged = true;
                }
                else
                {
                    var qComment = string.IsNullOrWhiteSpace(entry.QuizComment) ? null : entry.QuizComment.Trim();
                    var mComment = string.IsNullOrWhiteSpace(entry.MidTermComment) ? null : entry.MidTermComment.Trim();
                    var fComment = string.IsNullOrWhiteSpace(entry.FinalComment) ? null : entry.FinalComment.Trim();

                    if (transcript.QuizScore != entry.QuizScore ||
                        transcript.QuizComment != qComment ||
                        transcript.MidTermScore != entry.MidTermScore ||
                        transcript.MidTermComment != mComment ||
                        transcript.FinalScore != entry.FinalScore ||
                        transcript.FinalComment != fComment ||
                        transcript.FinalScoreTotal != finalScoreTotal)
                    {
                        transcript.QuizScore = entry.QuizScore;
                        transcript.QuizComment = qComment;
                        transcript.MidTermScore = entry.MidTermScore;
                        transcript.MidTermComment = mComment;
                        transcript.FinalScore = entry.FinalScore;
                        transcript.FinalComment = fComment;
                        transcript.FinalScoreTotal = finalScoreTotal;
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    saved++;

                    // Gửi thông báo cập nhật bảng điểm định kỳ cho phụ huynh qua SignalR
                    if (classStudent.Student.ParentId > 0)
                    {
                        try
                        {
                            await _notificationService.NotifyClassTranscriptUpdatedAsync(
                                classStudent.Student.ParentId,
                                classId,
                                classEntity.ClassName,
                                classStudent.Student.FullName,
                                entry.QuizScore,
                                entry.QuizComment,
                                entry.MidTermScore,
                                entry.MidTermComment,
                                entry.FinalScore,
                                entry.FinalComment,
                                finalScoreTotal);
                        }
                        catch (Exception)
                        {
                            // Ghi log lỗi gửi thông báo nhưng không làm gián đoạn việc lưu điểm
                        }
                    }
                }
            }

            // Kiểm tra nếu tất cả học sinh trong lớp đã được nhập đầy đủ 3 đầu điểm (Quiz, Giữa kỳ, Cuối kỳ) -> Chuyển trạng thái lớp sang "Completed" (Đã hoàn thành)
            var allStudentIds = classStudents.Select(cs => cs.StudentId).ToList();
            var allTranscripts = await _context.ClassTranscripts
                .Where(ct => ct.ClassId == classId && allStudentIds.Contains(ct.StudentId))
                .ToListAsync();

            if (allStudentIds.Any() && allTranscripts.Count == allStudentIds.Count &&
                allTranscripts.All(ct => ct.QuizScore.HasValue && ct.MidTermScore.HasValue && ct.FinalScore.HasValue))
            {
                classEntity.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã lưu thành công bảng điểm định kỳ của {saved} học sinh.",
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
    }

    public class TranscriptEntryDto
    {
        public int StudentId { get; set; }
        public decimal? QuizScore { get; set; }
        public string? QuizComment { get; set; }
        public decimal? MidTermScore { get; set; }
        public string? MidTermComment { get; set; }
        public decimal? FinalScore { get; set; }
        public string? FinalComment { get; set; }
    }
}

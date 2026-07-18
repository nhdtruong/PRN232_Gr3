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

            var lessonIds = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .Select(l => l.Id)
                .ToListAsync();

            var allDailyAssessments = await _context.DailyAssessments
                .Where(da => lessonIds.Contains(da.LessonId))
                .ToListAsync();

            int saved = 0;

            foreach (var entry in entries)
            {
                if (!classStudentsMap.TryGetValue(entry.StudentId, out var classStudent))
                    continue;

                if (entry.MidTermScore.HasValue && (entry.MidTermScore < 0 || entry.MidTermScore > 10))
                    return BadRequest(new { message = $"Điểm giữa kỳ phải từ 0-10 (StudentId={entry.StudentId})" });

                if (entry.FinalScore.HasValue && (entry.FinalScore < 0 || entry.FinalScore > 10))
                    return BadRequest(new { message = $"Điểm cuối kỳ phải từ 0-10 (StudentId={entry.StudentId})" });

                // Tính toán AverageDailyScore (trung bình TX)
                var studentTXScores = allDailyAssessments
                    .Where(da => da.StudentId == entry.StudentId && da.Score.HasValue)
                    .Select(da => da.Score!.Value)
                    .ToList();

                decimal? avgDailyScore = studentTXScores.Any()
                    ? Math.Round(studentTXScores.Average(), 2)
                    : null;

                // Tính toán FinalScoreTotal nếu đủ 3 đầu điểm
                decimal? finalScoreTotal = null;
                if (avgDailyScore.HasValue && entry.MidTermScore.HasValue && entry.FinalScore.HasValue)
                {
                    finalScoreTotal = Math.Round(avgDailyScore.Value * 0.3m + entry.MidTermScore.Value * 0.3m + entry.FinalScore.Value * 0.4m, 2);
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
                        MidTermScore = entry.MidTermScore,
                        MidTermComment = entry.MidTermComment,
                        FinalScore = entry.FinalScore,
                        FinalComment = entry.FinalComment,
                        AverageDailyScore = avgDailyScore,
                        FinalScoreTotal = finalScoreTotal
                    };
                    _context.ClassTranscripts.Add(transcript);
                    isChanged = true;
                }
                else
                {
                    if (transcript.MidTermScore != entry.MidTermScore ||
                        transcript.MidTermComment != entry.MidTermComment ||
                        transcript.FinalScore != entry.FinalScore ||
                        transcript.FinalComment != entry.FinalComment ||
                        transcript.AverageDailyScore != avgDailyScore ||
                        transcript.FinalScoreTotal != finalScoreTotal)
                    {
                        transcript.MidTermScore = entry.MidTermScore;
                        transcript.MidTermComment = entry.MidTermComment;
                        transcript.FinalScore = entry.FinalScore;
                        transcript.FinalComment = entry.FinalComment;
                        transcript.AverageDailyScore = avgDailyScore;
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
                                entry.MidTermScore,
                                entry.MidTermComment,
                                entry.FinalScore,
                                entry.FinalComment,
                                avgDailyScore,
                                finalScoreTotal);
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
        public decimal? MidTermScore { get; set; }
        public string? MidTermComment { get; set; }
        public decimal? FinalScore { get; set; }
        public string? FinalComment { get; set; }
    }
}

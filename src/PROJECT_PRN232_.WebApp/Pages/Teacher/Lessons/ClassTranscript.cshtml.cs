using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class ClassTranscriptModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClassTranscriptModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        public string? ClassName { get; set; }
        public string? ErrorMessage { get; set; }
        public List<StudentTranscriptRow> Students { get; set; } = new();

        public class StudentTranscriptRow
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public decimal? AverageDailyScore { get; set; }
            public decimal? MidTermScore { get; set; }
            public string? MidTermComment { get; set; }
            public decimal? FinalScore { get; set; }
            public string? FinalComment { get; set; }
            public decimal? FinalScoreTotal { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (ClassId <= 0)
            {
                ErrorMessage = "Mã lớp học không hợp lệ.";
                return Page();
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int teacherId))
            {
                ErrorMessage = "Không thể xác định thông tin đăng nhập.";
                return Page();
            }

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == ClassId && c.TeacherId == teacherId);
            if (classEntity == null)
            {
                ErrorMessage = "Bạn không có quyền truy cập lớp học này.";
                return Page();
            }

            ClassName = classEntity.ClassName;

            // Lấy tất cả học sinh trong lớp
            var classStudents = await _context.ClassStudents
                .Where(cs => cs.ClassId == ClassId)
                .Include(cs => cs.Student)
                .OrderBy(cs => cs.Student.FullName)
                .ToListAsync();

            if (!classStudents.Any())
            {
                return Page();
            }

            // Lấy danh sách lessonId của lớp này
            var lessonIds = await _context.Lessons
                .Where(l => l.ClassId == ClassId)
                .Select(l => l.Id)
                .ToListAsync();

            // Lấy tất cả DailyAssessment của các buổi học trong lớp
            var allDailyAssessments = await _context.DailyAssessments
                .Where(da => lessonIds.Contains(da.LessonId) && da.Score.HasValue)
                .ToListAsync();

            // Lấy bảng điểm định kỳ đã lưu
            var savedTranscripts = await _context.ClassTranscripts
                .Where(ct => ct.ClassId == ClassId)
                .ToDictionaryAsync(ct => ct.StudentId);

            foreach (var cs in classStudents)
            {
                // Tính trung bình TX
                var studentTXScores = allDailyAssessments
                    .Where(da => da.StudentId == cs.StudentId)
                    .Select(da => da.Score!.Value)
                    .ToList();

                decimal? avgDailyScore = studentTXScores.Any()
                    ? Math.Round(studentTXScores.Average(), 2)
                    : (decimal?)null;

                savedTranscripts.TryGetValue(cs.StudentId, out var transcript);

                var row = new StudentTranscriptRow
                {
                    StudentId = cs.StudentId,
                    StudentName = cs.Student.FullName,
                    AverageDailyScore = avgDailyScore,
                    MidTermScore = transcript?.MidTermScore,
                    MidTermComment = transcript?.MidTermComment,
                    FinalScore = transcript?.FinalScore,
                    FinalComment = transcript?.FinalComment
                };

                // Tự động tính điểm tổng kết lớp học nếu có đủ cả 3 cột điểm
                if (row.AverageDailyScore.HasValue && row.MidTermScore.HasValue && row.FinalScore.HasValue)
                {
                    row.FinalScoreTotal = Math.Round(row.AverageDailyScore.Value * 0.3m + row.MidTermScore.Value * 0.3m + row.FinalScore.Value * 0.4m, 2);
                }
                else
                {
                    row.FinalScoreTotal = transcript?.FinalScoreTotal;
                }

                Students.Add(row);
            }

            return Page();
        }
    }
}

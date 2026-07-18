using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class ClassGradeSheetModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClassGradeSheetModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int HighlightLessonId { get; set; }

        public string? ClassName { get; set; }
        public string? ErrorMessage { get; set; }
        public List<StudentGradeRow> Students { get; set; } = new();

        // Các đầu điểm thường xuyên (tối đa 5 đầu điểm từ các buổi học)
        public List<LessonScoreHeader> LessonHeaders { get; set; } = new();

        public class LessonScoreHeader
        {
            public int LessonId { get; set; }
            public string LessonTitle { get; set; } = "";
            public string LessonDate { get; set; } = "";
        }

        public class StudentGradeRow
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            // Điểm thường xuyên: key = LessonId, value = (AssessmentId, Score, Comment)
            public Dictionary<int, (int? AssessmentId, decimal? Score, string? Comment)> TXScores { get; set; } = new();
            public (int? AssessmentId, decimal? Score, string? Comment) GiuaKy { get; set; }
            public (int? AssessmentId, decimal? Score, string? Comment) CuoiKy { get; set; }
            public decimal? TrungBinhTX => TXScores.Count > 0 && TXScores.Values.Any(v => v.Score.HasValue)
                ? TXScores.Values.Where(v => v.Score.HasValue).Average(v => v.Score!.Value)
                : null;
            public decimal? DiemTongKet => CalcTongKet();

            private decimal? CalcTongKet()
            {
                var tx = TrungBinhTX;
                var gk = GiuaKy.Score;
                var ck = CuoiKy.Score;
                if (tx == null && gk == null && ck == null) return null;
                var sum = (tx ?? 0) * 0.3m + (gk ?? 0) * 0.3m + (ck ?? 0) * 0.4m;
                return Math.Round(sum, 2);
            }
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

            // Kiểm tra quyền giáo viên
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == ClassId && c.TeacherId == teacherId);
            if (classEntity == null)
            {
                ErrorMessage = "Bạn không có quyền truy cập lớp học này.";
                return Page();
            }

            ClassName = classEntity.ClassName;

            // Lấy các buổi học của lớp (chỉ lấy buổi được highlight nếu có)
            var queryLessons = _context.Lessons
                .Where(l => l.ClassId == ClassId);

            if (HighlightLessonId > 0)
            {
                queryLessons = queryLessons.Where(l => l.Id == HighlightLessonId);
            }

            var lessons = await queryLessons
                .OrderBy(l => l.LessonDate)
                .Select(l => new { l.Id, l.LessonDate, l.Title })
                .ToListAsync();

            LessonHeaders = lessons.Select((l, idx) => new LessonScoreHeader
            {
                LessonId = l.Id,
                LessonTitle = l.Title ?? $"Buổi {idx + 1}",
                LessonDate = l.LessonDate.ToString("dd/MM")
            }).ToList();

            // Lấy tất cả học sinh trong lớp
            var classStudents = await _context.ClassStudents
                .Where(cs => cs.ClassId == ClassId)
                .Include(cs => cs.Student)
                .OrderBy(cs => cs.Student.FullName)
                .ToListAsync();

            if (!classStudents.Any())
            {
                Students = new List<StudentGradeRow>();
                return Page();
            }

            var lessonIds = lessons.Select(l => l.Id).ToList();
            var studentIds = classStudents.Select(cs => cs.StudentId).ToList();

            // Lấy toàn bộ DailyAssessment của học sinh trong lớp cho các buổi học này
            var allAssessments = await _context.DailyAssessments
                .Where(a => studentIds.Contains(a.StudentId) && lessonIds.Contains(a.LessonId))
                .ToListAsync();

            foreach (var cs in classStudents)
            {
                var row = new StudentGradeRow
                {
                    StudentId = cs.StudentId,
                    StudentName = cs.Student.FullName
                };

                var studentAssessments = allAssessments.Where(a => a.StudentId == cs.StudentId).ToList();

                // Điểm TX - theo từng buổi học
                foreach (var lid in lessonIds)
                {
                    var a = studentAssessments.FirstOrDefault(x => x.LessonId == lid);
                    row.TXScores[lid] = (a?.Id, a?.Score, a?.Comment);
                }

                // Không hiển thị GK và CK trên trang này
                row.GiuaKy = (null, null, null);
                row.CuoiKy = (null, null, null);

                Students.Add(row);
            }

            return Page();
        }
    }
}

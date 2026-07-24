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
            if (ClassId > 0)
            {
                return Redirect("/Teacher/Lessons/ClassTranscript?ClassId=" + ClassId);
            }
            return Redirect("/Teacher/MyClasses");
        }
    }
}

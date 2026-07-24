using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Parent
{
    [Authorize(Roles = "Parent")]
    public class GradesModel : PageModel
    {
        private readonly AppDbContext _context;

        public GradesModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedStudentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ClassId { get; set; }

        public List<StudentOption> Children { get; set; } = new();
        public StudentOption? CurrentStudent { get; set; }
        public List<ClassTranscriptViewModel> ClassGrades { get; set; } = new();

        public class StudentOption
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
        }

        public class ClassTranscriptViewModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string? Subject { get; set; }
            public string? TeacherName { get; set; }
            public string Status { get; set; } = "Active";

            public decimal? QuizScore { get; set; }
            public string? QuizComment { get; set; }
            public decimal? MidTermScore { get; set; }
            public string? MidTermComment { get; set; }
            public decimal? FinalScore { get; set; }
            public string? FinalComment { get; set; }
            public decimal? FinalScoreTotal { get; set; }
            public string AcademicRating { get; set; } = "Chưa xếp loại";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int parentId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var children = await _context.Students
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.FullName)
                .Select(s => new StudentOption
                {
                    Id = s.Id,
                    FullName = s.FullName
                })
                .ToListAsync();

            Children = children;

            if (!Children.Any())
            {
                return Page();
            }

            if (!SelectedStudentId.HasValue || !Children.Any(c => c.Id == SelectedStudentId.Value))
            {
                SelectedStudentId = Children.First().Id;
            }

            CurrentStudent = Children.FirstOrDefault(c => c.Id == SelectedStudentId.Value);

            var studentId = SelectedStudentId.Value;

            // Fetch enrolled classes
            var classQuery = _context.ClassStudents
                .Where(cs => cs.StudentId == studentId)
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.Teacher)
                .AsQueryable();

            if (ClassId.HasValue && ClassId.Value > 0)
            {
                classQuery = classQuery.Where(cs => cs.ClassId == ClassId.Value);
            }

            var enrolledClasses = await classQuery.ToListAsync();

            if (!enrolledClasses.Any())
            {
                return Page();
            }

            var enrolledClassIds = enrolledClasses.Select(cs => cs.ClassId).ToList();

            // Fetch transcripts
            var transcripts = await _context.ClassTranscripts
                .Where(ct => ct.StudentId == studentId && enrolledClassIds.Contains(ct.ClassId))
                .ToDictionaryAsync(ct => ct.ClassId);

            foreach (var cs in enrolledClasses)
            {
                var c = cs.Class;
                transcripts.TryGetValue(c.Id, out var transcript);

                decimal? quiz = transcript?.QuizScore;
                decimal? midTerm = transcript?.MidTermScore;
                decimal? final = transcript?.FinalScore;
                decimal? total = null;

                if (quiz.HasValue && midTerm.HasValue && final.HasValue)
                {
                    total = Math.Round(quiz.Value * 0.3m + midTerm.Value * 0.3m + final.Value * 0.4m, 2);
                }
                else
                {
                    total = transcript?.FinalScoreTotal;
                }

                string rating = "Chưa xếp loại";
                if (total.HasValue)
                {
                    if (total >= 8.5m) rating = "Xuất sắc";
                    else if (total >= 8.0m) rating = "Giỏi";
                    else if (total >= 6.5m) rating = "Khá";
                    else if (total >= 5.0m) rating = "Trung bình";
                    else rating = "Yếu";
                }

                string displayStatus = (c.Status == "Completed" || c.Status == "Đã hoàn thành" || (quiz.HasValue && midTerm.HasValue && final.HasValue))
                    ? "Completed"
                    : c.Status;

                ClassGrades.Add(new ClassTranscriptViewModel
                {
                    ClassId = c.Id,
                    ClassName = c.ClassName,
                    Subject = c.Subject,
                    TeacherName = c.Teacher?.FullName ?? "Chưa phân công",
                    Status = displayStatus,
                    QuizScore = quiz,
                    QuizComment = transcript?.QuizComment,
                    MidTermScore = midTerm,
                    MidTermComment = transcript?.MidTermComment,
                    FinalScore = final,
                    FinalComment = transcript?.FinalComment,
                    FinalScoreTotal = total,
                    AcademicRating = rating
                });
            }

            return Page();
        }
    }
}

using PROJECT_PRN232_.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly AppDbContext _context;

        public AssessmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assessment>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Assessments
                .Include(a => a.Student)
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<Assessment?> GetByStudentAndLessonAsync(int studentId, int lessonId)
        {
            return await _context.Assessments
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.LessonId == lessonId);
        }

        public async Task UpsertBulkAsync(int lessonId, IEnumerable<Assessment> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0) return;

            var studentIds = itemList.Select(i => i.StudentId).ToList();
            var existing = await _context.Assessments
                .Where(a => a.LessonId == lessonId && studentIds.Contains(a.StudentId))
                .ToListAsync();

            var existingMap = existing.ToDictionary(a => a.StudentId);

            foreach (var item in itemList)
            {
                item.LessonId = lessonId;
                item.DateAssessed = DateTime.Now;

                if (existingMap.TryGetValue(item.StudentId, out var found))
                {
                    found.Score = item.Score;
                    found.TeacherComment = item.TeacherComment;
                    found.DateAssessed = item.DateAssessed;
                }
                else
                {
                    await _context.Assessments.AddAsync(item);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Assessments
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Class)
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.Lesson.LessonDate)
                .ToListAsync();
        }
    }
}

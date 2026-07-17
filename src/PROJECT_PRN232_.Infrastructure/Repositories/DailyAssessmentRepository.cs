using PROJECT_PRN232_.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class DailyAssessmentRepository : IDailyAssessmentRepository
    {
        private readonly AppDbContext _context;

        public DailyAssessmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DailyAssessment>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.DailyAssessments
                .Include(a => a.Student)
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<DailyAssessment?> GetByStudentAndLessonAsync(int studentId, int lessonId)
        {
            return await _context.DailyAssessments
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.LessonId == lessonId);
        }

        public async Task UpsertBulkAsync(int lessonId, IEnumerable<DailyAssessment> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0) return;

            var studentIds = itemList.Select(i => i.StudentId).ToList();
            var existing = await _context.DailyAssessments
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
                    found.Comment = item.Comment;
                    found.DateAssessed = item.DateAssessed;
                }
                else
                {
                    await _context.DailyAssessments.AddAsync(item);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DailyAssessment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.DailyAssessments
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Class)
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.Lesson.LessonDate)
                .ToListAsync();
        }
    }
}

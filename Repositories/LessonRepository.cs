using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson?> GetLessonWithClassAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Class)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }

        public async Task<HashSet<int>> GetEnrolledStudentIdsAsync(int classId)
        {
            var ids = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<IEnumerable<(Student Student, Attendance? Attendance, Assessment? Assessment)>> GetRollCallDataAsync(
            int lessonId, int classId, int? parentIdFilter = null)
        {
            var query = _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Select(cs => cs.Student);

            if (parentIdFilter.HasValue)
            {
                query = query.Where(s => s.ParentId == parentIdFilter.Value);
            }

            var students = await query
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var attendances = await _context.Attendances
                .Where(a => a.LessonId == lessonId)
                .ToDictionaryAsync(a => a.StudentId);

            var assessments = await _context.Assessments
                .Where(a => a.LessonId == lessonId)
                .ToDictionaryAsync(a => a.StudentId);

            return students.Select(s => (
                s,
                attendances.GetValueOrDefault(s.Id),
                assessments.GetValueOrDefault(s.Id)
            ));
        }
    }
}

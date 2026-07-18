using PROJECT_PRN232_.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Repositories
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
                .Include(l => l.Room)
                .Include(l => l.Slot)
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

        public async Task<IEnumerable<(Student Student, Attendance? Attendance, DailyAssessment? Assessment)>> GetRollCallDataAsync(
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

            var assessments = await _context.DailyAssessments
                .Where(a => a.LessonId == lessonId)
                .ToDictionaryAsync(a => a.StudentId);

            return students.Select(s => (
                s,
                attendances.GetValueOrDefault(s.Id),
                assessments.GetValueOrDefault(s.Id)
            ));
        }
        public async Task<IEnumerable<Lesson>> GetByClassIdAsync(int classId)
        {
            return await _context.Lessons
                .Include(l => l.Class)
                .Include(l => l.Room)
                .Include(l => l.Slot)
                .Where(l => l.ClassId == classId)
                .OrderBy(l => l.LessonDate)
                .ToListAsync();
        }

        public async Task<Lesson> CreateAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<bool> UpdateAsync(Lesson lesson)
        {
            _context.Entry(lesson).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Class?> GetClassByIdAsync(int classId)
        {
            return await _context.Classes.FindAsync(classId);
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByStudentIdAsync(int studentId)
        {
            // Lấy danh sách các classId học sinh đó tham gia
            var classIds = await _context.ClassStudents
                .Where(cs => cs.StudentId == studentId)
                .Select(cs => cs.ClassId)
                .ToListAsync();

            return await _context.Lessons
                .Include(l => l.Class)
                .Include(l => l.Room)
                .Include(l => l.Slot)
                .Where(l => classIds.Contains(l.ClassId))
                .OrderBy(l => l.LessonDate)
                .ToListAsync();
        }

        public async Task<bool> PublishLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null) return false;
            lesson.IsPublished = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Lesson?> GetLessonWithMaterialsAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Class)
                .Include(l => l.Materials)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }

        public async Task<bool> CheckCollisionAsync(System.DateTime date, int slotId, int roomId, int? excludeLessonId = null)
        {
            // Trả về true nếu có bất kỳ lesson nào cùng ngày, ca học, phòng học (ngoại trừ excludeLessonId nếu sửa)
            var dateOnly = date.Date;
            return await _context.Lessons
                .AnyAsync(l => l.LessonDate.Date == dateOnly 
                            && l.SlotId == slotId 
                            && l.RoomId == roomId 
                            && (!excludeLessonId.HasValue || l.Id != excludeLessonId.Value));
        }
    }
}

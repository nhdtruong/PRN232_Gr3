using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _context;

        public AttendanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByStudentAndLessonAsync(int studentId, int lessonId)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.LessonId == lessonId);
        }

        public async Task UpsertBulkAsync(int lessonId, IEnumerable<Attendance> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0) return;

            var studentIds = itemList.Select(i => i.StudentId).ToList();
            var existing = await _context.Attendances
                .Where(a => a.LessonId == lessonId && studentIds.Contains(a.StudentId))
                .ToListAsync();

            var existingMap = existing.ToDictionary(a => a.StudentId);

            foreach (var item in itemList)
            {
                item.LessonId = lessonId;
                item.UpdatedAt = DateTime.Now;

                if (existingMap.TryGetValue(item.StudentId, out var found))
                {
                    found.Status = item.Status;
                    found.Note = item.Note;
                    found.UpdatedAt = item.UpdatedAt;
                }
                else
                {
                    await _context.Attendances.AddAsync(item);
                }
            }

            await _context.SaveChangesAsync();
        }

        // Thực thi mới cho Người 4: Lấy lịch sử điểm danh của học sinh
        public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Class)
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.Lesson.LessonDate)
                .ToListAsync();
        }

        // Thực thi mới cho Người 4: Sửa 1 bản ghi điểm danh
        public async Task<bool> UpdateSingleAsync(Attendance attendance)
        {
            var existing = await _context.Attendances.FindAsync(attendance.Id);
            if (existing == null) return false;

            existing.Status = attendance.Status;
            existing.Note = attendance.Note;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

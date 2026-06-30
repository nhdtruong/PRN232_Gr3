using PROJECT_PRN232_.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly AppDbContext _context;

        public ClassRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Class>> GetAllClassesAsync()
        {
            return await _context.Classes
                .Include(c => c.ClassStudents)
                .ToListAsync();
        }

        public async Task<Class?> GetClassByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.ClassStudents)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Class> AddClassAsync(Class classEntity)
        {
            await _context.Classes.AddAsync(classEntity);
            await _context.SaveChangesAsync();
            return classEntity;
        }

        public async Task<bool> UpdateClassAsync(Class classEntity)
        {
            _context.Entry(classEntity).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClassExistsAsync(classEntity.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.ClassStudents)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Attendances)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Assessments)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Notifications)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return false;
            }

            // Remove associated Notifications
            if (classEntity.Notifications != null && classEntity.Notifications.Any())
            {
                _context.Notifications.RemoveRange(classEntity.Notifications);
            }

            // Remove associated ClassStudent records (Enrollments)
            if (classEntity.ClassStudents != null && classEntity.ClassStudents.Any())
            {
                _context.ClassStudents.RemoveRange(classEntity.ClassStudents);
            }

            // Remove all nested entities within Lessons
            if (classEntity.Lessons != null && classEntity.Lessons.Any())
            {
                foreach (var lesson in classEntity.Lessons)
                {
                    if (lesson.Attendances != null && lesson.Attendances.Any())
                    {
                        _context.Attendances.RemoveRange(lesson.Attendances);
                    }
                    if (lesson.Assessments != null && lesson.Assessments.Any())
                    {
                        _context.Assessments.RemoveRange(lesson.Assessments);
                    }
                    if (lesson.Materials != null && lesson.Materials.Any())
                    {
                        _context.Materials.RemoveRange(lesson.Materials);
                    }
                }
                _context.Lessons.RemoveRange(classEntity.Lessons);
            }

            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ClassExistsAsync(int id)
        {
            return await _context.Classes.AnyAsync(e => e.Id == id);
        }
    }
}

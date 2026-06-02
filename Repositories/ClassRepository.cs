using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
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
            return await _context.Classes.ToListAsync();
        }

        public async Task<Class?> GetClassByIdAsync(int id)
        {
            return await _context.Classes.FindAsync(id);
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
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null)
            {
                return false;
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

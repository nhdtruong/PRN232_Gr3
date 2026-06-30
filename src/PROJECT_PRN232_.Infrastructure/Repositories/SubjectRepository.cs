using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _context;

        public SubjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetByCenterIdAsync(int centerId)
        {
            return await _context.Subjects
                .Include(s => s.Materials)
                .Where(s => s.CenterId == centerId)
                .OrderBy(s => s.SubjectCode)
                .ToListAsync();
        }

        public async Task<Subject?> GetByIdAsync(int subjectId)
        {
            return await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == subjectId);
        }

        public async Task<Subject?> GetByIdWithMaterialsAsync(int subjectId)
        {
            return await _context.Subjects
                .Include(s => s.Materials)
                .FirstOrDefaultAsync(s => s.Id == subjectId);
        }

        public async Task<bool> IsCodeDuplicateAsync(int centerId, string code, int? excludeId = null)
        {
            return await _context.Subjects
                .AnyAsync(s => s.CenterId == centerId
                            && s.SubjectCode == code
                            && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        public async Task<Subject> CreateAsync(Subject subject)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<bool> UpdateAsync(Subject subject)
        {
            _context.Entry(subject).State = EntityState.Modified;
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

        public async Task<bool> DeleteAsync(int subjectId)
        {
            var subject = await _context.Subjects
                .Include(s => s.Materials)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null) return false;

            // Xóa toàn bộ tài liệu thuộc môn học trước
            _context.Materials.RemoveRange(subject.Materials);
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

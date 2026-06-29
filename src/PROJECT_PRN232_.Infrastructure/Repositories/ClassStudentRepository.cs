using PROJECT_PRN232_.Application.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class ClassStudentRepository : IClassStudentRepository
    {
        private readonly AppDbContext _context;

        public ClassStudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassStudent>> GetStudentsByClassIdAsync(int classId)
        {
            return await _context.ClassStudents
                .Include(cs => cs.Student)
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassStudent?> GetEnrollmentAsync(int classId, int studentId)
        {
            return await _context.ClassStudents
                .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        }

        public async Task<ClassStudent> AddAsync(ClassStudent classStudent)
        {
            await _context.ClassStudents.AddAsync(classStudent);
            await _context.SaveChangesAsync();
            return classStudent;
        }

        public async Task<bool> RemoveAsync(int classId, int studentId)
        {
            var enrollment = await GetEnrollmentAsync(classId, studentId);
            if (enrollment == null)
            {
                return false;
            }
            _context.ClassStudents.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ClassStudent>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.ClassStudents
                .Include(cs => cs.Class)
                .Where(cs => cs.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<int>> GetParentIdsInClassAsync(int classId)
        {
            return await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Join(_context.Students,
                      cs => cs.StudentId,
                      s => s.Id,
                      (cs, s) => s.ParentId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Student?> GetStudentInClassForParentAsync(int classId, int parentId)
        {
            return await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Join(_context.Students.Where(s => s.ParentId == parentId),
                      cs => cs.StudentId,
                      s => s.Id,
                      (cs, s) => s)
                .FirstOrDefaultAsync();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public interface IClassStudentRepository
    {
        Task<IEnumerable<ClassStudent>> GetStudentsByClassIdAsync(int classId);
        Task<ClassStudent?> GetEnrollmentAsync(int classId, int studentId);
        Task<ClassStudent> AddAsync(ClassStudent classStudent);
        Task<bool> RemoveAsync(int classId, int studentId);
        Task<IEnumerable<ClassStudent>> GetEnrollmentsByStudentIdAsync(int studentId);
    }
}

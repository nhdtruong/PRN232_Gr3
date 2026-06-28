using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<Student>> GetStudentsInClassAsync(int classId);
        Task<bool> EnrollStudentAsync(int classId, int studentId);
        Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
        Task<bool> TransferStudentClassAsync(int studentId, int fromClassId, int toClassId);
        Task<IEnumerable<Class>> GetClassesForStudentAsync(int studentId);
    }
}

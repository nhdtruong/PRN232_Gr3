using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<Student>> GetStudentsInClassAsync(int classId);
        Task<bool> EnrollStudentAsync(int classId, int studentId);
        Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
        Task<bool> TransferStudentClassAsync(int studentId, int fromClassId, int toClassId);
        Task<IEnumerable<Class>> GetClassesForStudentAsync(int studentId, int? parentUserId = null);
    }
}

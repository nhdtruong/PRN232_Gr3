using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IClassStudentRepository
    {
        Task<IEnumerable<ClassStudent>> GetStudentsByClassIdAsync(int classId);
        Task<ClassStudent?> GetEnrollmentAsync(int classId, int studentId);
        Task<ClassStudent> AddAsync(ClassStudent classStudent);
        Task<bool> RemoveAsync(int classId, int studentId);
        Task<IEnumerable<ClassStudent>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task<List<int>> GetParentIdsInClassAsync(int classId);
        Task<Student?> GetStudentInClassForParentAsync(int classId, int parentId);
    }
}

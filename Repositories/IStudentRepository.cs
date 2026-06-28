using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int id);
        Task<IEnumerable<Student>> GetStudentsByParentIdAsync(int parentId);
    }
}

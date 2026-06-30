using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<Subject>> GetByCenterIdAsync(int centerId);
        Task<Subject?> GetByIdAsync(int subjectId);
        Task<Subject?> GetByIdWithMaterialsAsync(int subjectId);
        Task<bool> IsCodeDuplicateAsync(int centerId, string code, int? excludeId = null);
        Task<Subject> CreateAsync(Subject subject);
        Task<bool> UpdateAsync(Subject subject);
        Task<bool> DeleteAsync(int subjectId);
    }
}

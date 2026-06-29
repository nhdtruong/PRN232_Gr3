using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IMaterialRepository
    {
        Task<IEnumerable<Material>> GetByLessonIdAsync(int lessonId);
        Task<Material?> GetByIdAsync(int materialId);
        Task<Material> CreateAsync(Material material);
        Task<bool> DeleteAsync(int materialId);
    }
}

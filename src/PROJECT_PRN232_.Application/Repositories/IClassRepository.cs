using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IClassRepository
    {
        Task<IEnumerable<Class>> GetAllClassesAsync();
        Task<Class?> GetClassByIdAsync(int id);
        Task<Class> AddClassAsync(Class classEntity);
        Task<bool> UpdateClassAsync(Class classEntity);
        Task<bool> DeleteClassAsync(int id);
    }
}

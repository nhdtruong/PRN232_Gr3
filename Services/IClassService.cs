using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface IClassService
    {
        Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync();
        Task<ClassResponseDto?> GetClassByIdAsync(int id);
        Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto);
        Task<bool> UpdateClassAsync(ClassUpdateDto dto);
        Task<bool> DeleteClassAsync(int id);
    }
}

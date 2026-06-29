using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IClassService
    {
        Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync();
        Task<ClassResponseDto?> GetClassByIdAsync(int id);
        Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto);
        Task<bool> UpdateClassAsync(ClassUpdateDto dto);
        Task<bool> UpdateClassStatusAsync(int id, string status);
        Task<bool> DeleteClassAsync(int id);
    }
}

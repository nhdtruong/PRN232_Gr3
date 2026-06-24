using System.Threading.Tasks;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public interface IParentProfileService
    {
        Task<ParentProfileDto?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, ParentProfileUpdateDto dto);
    }
}

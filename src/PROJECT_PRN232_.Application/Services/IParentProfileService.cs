using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface IParentProfileService
    {
        Task<ParentProfileDto?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, ParentProfileUpdateDto dto);
    }
}

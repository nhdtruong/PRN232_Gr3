using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class ParentProfileService : IParentProfileService
    {
        private readonly IStudentRepository _studentRepository;

        public ParentProfileService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<ParentProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _studentRepository.GetParentUserByIdAsync(userId);
            if (user == null) return null;

            return new ParentProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, ParentProfileUpdateDto dto)
        {
            var user = await _studentRepository.GetParentUserByIdAsync(userId);
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            return await _studentRepository.UpdateParentUserAsync(user);
        }
    }
}

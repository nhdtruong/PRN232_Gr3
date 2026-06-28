using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Services
{
    public class ParentProfileService : IParentProfileService
    {
        private readonly AppDbContext _context;

        public ParentProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ParentProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == "Parent");
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == "Parent");
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Teacher.Chat
{
    [Authorize(Roles = "Teacher")]
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IChatService _chatService;

        public IndexModel(AppDbContext context, IChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        public User CenterAdmin { get; set; } = null!;
        public int ChannelId { get; set; }
        public int CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Get the Center administrator (seed username: admin)
            var centerAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Center" && u.IsActive);

            if (centerAdmin == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy quản trị viên Trung tâm.";
                return RedirectToPage("/Teacher/Dashboard");
            }

            CenterAdmin = centerAdmin;

            // Get or create channel between this teacher and the center admin
            var channelDto = await _chatService.GetOrCreateChannelAsync(centerAdmin.Id, CurrentUserId);
            ChannelId = channelDto.Id;

            return Page();
        }
    }
}

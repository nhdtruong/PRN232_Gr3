using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Pages.Center.Chat
{
    [Authorize(Roles = "Center")]
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

        public List<User> Parents { get; set; } = new List<User>();
        public int CurrentUserId { get; set; }
        public int? TargetParentId { get; set; }

        public async Task OnGetAsync(int? parentId)
        {
            CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            TargetParentId = parentId;
            
            // Get all active parents
            Parents = await _context.Users
                .Where(u => u.Role == "Parent" && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostGetOrCreateChannelAsync([FromBody] CreateChannelRequest request)
        {
            var centerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var channelDto = await _chatService.GetOrCreateChannelAsync(centerId, request.ParentId);
            return new JsonResult(channelDto);
        }

        public class CreateChannelRequest
        {
            public int ParentId { get; set; }
        }
    }
}

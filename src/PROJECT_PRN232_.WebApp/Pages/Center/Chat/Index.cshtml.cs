using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Chat
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

        public class ParentChatViewModel
        {
            public int Id { get; set; }
            public string FullName { get; set; } = null!;
            public int UnreadCount { get; set; }
        }

        public List<ParentChatViewModel> Parents { get; set; } = new List<ParentChatViewModel>();
        public int CurrentUserId { get; set; }
        public int? TargetParentId { get; set; }

        public async Task OnGetAsync(int? parentId)
        {
            CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            TargetParentId = parentId;
            
            // Get all active parents with their unread message count
            Parents = await _context.Users
                .Where(u => u.Role == "Parent" && u.IsActive)
                .Select(u => new ParentChatViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UnreadCount = _context.ChatMessages
                        .Count(m => m.ChatChannel.CenterId == CurrentUserId &&
                                    m.ChatChannel.ParentId == u.Id &&
                                    m.SenderId == u.Id &&
                                    !m.IsRead)
                })
                .OrderByDescending(p => p.UnreadCount)
                .ThenBy(p => p.FullName)
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

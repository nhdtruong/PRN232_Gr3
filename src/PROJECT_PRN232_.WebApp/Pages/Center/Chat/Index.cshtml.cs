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
            public string StudentNames { get; set; } = string.Empty; // Tên con của phụ huynh
            public string ClassNames { get; set; } = string.Empty;  // Lớp học của con
        }

        public List<ParentChatViewModel> Parents { get; set; } = new List<ParentChatViewModel>();
        public List<string> AvailableClasses { get; set; } = new List<string>();
        public int CurrentUserId { get; set; }
        public int? TargetParentId { get; set; }

        public async Task OnGetAsync(int? parentId)
        {
            CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            TargetParentId = parentId;
            
            // Get all active parents with their unread message count, student names and class names
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
                                    !m.IsRead),
                    // Lấy tên con(s) của phụ huynh
                    StudentNames = string.Join(", ", _context.Students
                        .Where(s => s.ParentId == u.Id)
                        .Select(s => s.FullName)),
                    // Lấy tên lớp(s) của con(s) thuộc center này
                    ClassNames = string.Join(", ", _context.Students
                        .Where(s => s.ParentId == u.Id)
                        .SelectMany(s => s.ClassStudents)
                        .Where(cs => cs.Class.CenterId == CurrentUserId)
                        .Select(cs => cs.Class.ClassName)
                        .Distinct())
                })
                .OrderByDescending(p => p.UnreadCount)
                .ThenBy(p => p.FullName)
                .ToListAsync();

            // Lấy danh sách tất cả lớp của center này để hiện trong combobox
            AvailableClasses = await _context.Classes
                .Where(c => c.CenterId == CurrentUserId)
                .OrderBy(c => c.ClassName)
                .Select(c => c.ClassName)
                .Distinct()
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

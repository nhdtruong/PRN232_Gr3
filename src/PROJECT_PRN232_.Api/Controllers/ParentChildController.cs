using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/parent")]
    [Authorize(Roles = "Parent")]
    public class ParentChildController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParentChildController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách con (học sinh) của phụ huynh đang đăng nhập
        /// GET /api/parent/my-children
        /// </summary>
        [HttpGet("my-children")]
        public async Task<IActionResult> GetMyChildren()
        {
            var parentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(parentIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin phụ huynh." });
            }

            var children = await _context.Students
                .Where(s => s.ParentId == parentId)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender
                })
                .ToListAsync();

            return Ok(children);
        }
    }
}

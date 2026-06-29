using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/parent-profile")]
    [Authorize(Roles = "Parent")]
    public class ParentProfileController : ControllerBase
    {
        private readonly IParentProfileService _parentProfileService;

        public ParentProfileController(IParentProfileService parentProfileService)
        {
            _parentProfileService = parentProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var profile = await _parentProfileService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(new { message = "Không tìm thấy thông tin hồ sơ." });

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ParentProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var result = await _parentProfileService.UpdateProfileAsync(userId, dto);
            if (!result)
                return BadRequest(new { message = "Cập nhật hồ sơ thất bại." });

            return Ok(new { message = "Cập nhật hồ sơ thành công!" });
        }
    }
}

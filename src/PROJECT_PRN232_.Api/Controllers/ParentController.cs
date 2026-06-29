using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize(Roles = "Parent")]
    public class ParentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public ParentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet("api/parent/children/{studentId}/classes")]
        public async Task<IActionResult> GetChildClasses(int studentId)
        {
            var parentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(parentIdString, out int parentId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }
            var classes = await _enrollmentService.GetClassesForStudentAsync(studentId, parentId);
            return Ok(classes);
        }
    }
}

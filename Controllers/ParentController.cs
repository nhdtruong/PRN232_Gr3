using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Services;

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
            var classes = await _enrollmentService.GetClassesForStudentAsync(studentId);
            return Ok(classes);
        }
    }
}

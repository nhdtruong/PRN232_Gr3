using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/lessons")]
    [Authorize]
    public class LessonController : ControllerBase
    {
        public LessonController()
        {
        }

        // TODO: Viết API tạo buổi học ở đây
    }
}

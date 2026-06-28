using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/materials")]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        public MaterialController()
        {
        }

        // TODO: Định nghĩa các API endpoint của Materials cho Người 4 tại đây
    }
}

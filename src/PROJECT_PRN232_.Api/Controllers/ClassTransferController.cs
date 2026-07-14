using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassTransferController : ControllerBase
    {
        private readonly ITransferRequestService _transferRequestService;

        public ClassTransferController(ITransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] TransferRequestCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = await _transferRequestService.CreateRequestAsync(dto);
            return Ok(request);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetRequestsForTeacher(int teacherId)
        {
            var requests = await _transferRequestService.GetRequestsForTeacherAsync(teacherId);
            return Ok(requests);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetAllPendingRequests()
        {
            var requests = await _transferRequestService.GetAllPendingRequestsAsync();
            return Ok(requests);
        }

        [HttpPut("{id}/process")]
        public async Task<IActionResult> ProcessRequest(int id, [FromBody] TransferRequestStatusUpdateDto dto)
        {
            var result = await _transferRequestService.ProcessRequestAsync(id, dto.IsApproved);
            if (!result)
            {
                return BadRequest(new { message = "Không thể xử lý yêu cầu hoặc yêu cầu không tồn tại/đã được xử lý." });
            }

            return Ok(new { message = dto.IsApproved ? "Đã phê duyệt yêu cầu đổi lớp." : "Đã từ chối yêu cầu đổi lớp." });
        }
    }
}

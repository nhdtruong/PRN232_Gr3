using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.WebApp.Pages.Center
{
    [Authorize(Roles = "Center")]
    public class TransferRequestsModel : PageModel
    {
        private readonly ITransferRequestService _transferRequestService;

        public TransferRequestsModel(ITransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        public IEnumerable<ClassTransferRequestDto> PendingRequests { get; set; } = new List<ClassTransferRequestDto>();

        public async Task OnGetAsync()
        {
            PendingRequests = await _transferRequestService.GetAllPendingRequestsAsync();
        }
    }
}

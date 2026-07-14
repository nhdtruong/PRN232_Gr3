using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;

namespace PROJECT_PRN232_.Application.Services
{
    public interface ITransferRequestService
    {
        Task<ClassTransferRequestDto> CreateRequestAsync(TransferRequestCreateDto dto);
        Task<IEnumerable<ClassTransferRequestDto>> GetRequestsForTeacherAsync(int teacherId);
        Task<IEnumerable<ClassTransferRequestDto>> GetAllPendingRequestsAsync();
        Task<bool> ProcessRequestAsync(int requestId, bool isApproved);
    }
}

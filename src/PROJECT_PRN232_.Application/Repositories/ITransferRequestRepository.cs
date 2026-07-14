using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface ITransferRequestRepository
    {
        Task<ClassTransferRequest> AddAsync(ClassTransferRequest request);
        Task<ClassTransferRequest?> GetByIdAsync(int id);
        Task<IEnumerable<ClassTransferRequest>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<ClassTransferRequest>> GetPendingRequestsAsync();
        Task<bool> UpdateAsync(ClassTransferRequest request);
    }
}

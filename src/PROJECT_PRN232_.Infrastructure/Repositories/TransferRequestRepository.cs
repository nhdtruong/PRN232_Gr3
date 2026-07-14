using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Infrastructure.Data;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        private readonly AppDbContext _context;

        public TransferRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClassTransferRequest> AddAsync(ClassTransferRequest request)
        {
            _context.ClassTransferRequests.Add(request);
            await _context.SaveChangesAsync();

            return await _context.ClassTransferRequests
                .Include(r => r.Class)
                .Include(r => r.FromTeacher)
                .Include(r => r.ToTeacher)
                .FirstOrDefaultAsync(r => r.Id == request.Id) ?? request;
        }

        public async Task<ClassTransferRequest?> GetByIdAsync(int id)
        {
            return await _context.ClassTransferRequests
                .Include(r => r.Class)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<ClassTransferRequest>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.ClassTransferRequests
                .Include(r => r.Class)
                .Include(r => r.FromTeacher)
                .Include(r => r.ToTeacher)
                .Where(r => r.FromTeacherId == teacherId || r.ToTeacherId == teacherId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassTransferRequest>> GetPendingRequestsAsync()
        {
            return await _context.ClassTransferRequests
                .Include(r => r.Class)
                .Include(r => r.FromTeacher)
                .Include(r => r.ToTeacher)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(ClassTransferRequest request)
        {
            _context.ClassTransferRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

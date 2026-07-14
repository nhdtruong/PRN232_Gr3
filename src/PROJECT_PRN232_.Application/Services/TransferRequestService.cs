using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Services
{
    public class TransferRequestService : ITransferRequestService
    {
        private readonly ITransferRequestRepository _repository;
        private readonly IClassRepository _classRepository;

        public TransferRequestService(ITransferRequestRepository repository, IClassRepository classRepository)
        {
            _repository = repository;
            _classRepository = classRepository;
        }

        public async Task<ClassTransferRequestDto> CreateRequestAsync(TransferRequestCreateDto dto)
        {
            var request = new ClassTransferRequest
            {
                ClassId = dto.ClassId,
                FromTeacherId = dto.FromTeacherId,
                ToTeacherId = dto.ToTeacherId,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            var createdRequest = await _repository.AddAsync(request);

            return MapToDto(createdRequest);
        }

        public async Task<IEnumerable<ClassTransferRequestDto>> GetRequestsForTeacherAsync(int teacherId)
        {
            var requests = await _repository.GetByTeacherIdAsync(teacherId);
            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassTransferRequestDto>> GetAllPendingRequestsAsync()
        {
            var requests = await _repository.GetPendingRequestsAsync();
            return requests.Select(MapToDto);
        }

        public async Task<bool> ProcessRequestAsync(int requestId, bool isApproved)
        {
            var request = await _repository.GetByIdAsync(requestId);

            if (request == null || request.Status != "Pending") return false;

            request.Status = isApproved ? "Approved" : "Rejected";
            request.UpdatedAt = DateTime.Now;

            if (isApproved)
            {
                var classEntity = await _classRepository.GetClassByIdAsync(request.ClassId);
                if (classEntity != null)
                {
                    classEntity.TeacherId = request.ToTeacherId;
                    await _classRepository.UpdateClassAsync(classEntity);
                }
            }

            return await _repository.UpdateAsync(request);
        }

        private static ClassTransferRequestDto MapToDto(ClassTransferRequest request)
        {
            return new ClassTransferRequestDto
            {
                Id = request.Id,
                ClassId = request.ClassId,
                ClassName = request.Class?.ClassName ?? "Unknown",
                FromTeacherId = request.FromTeacherId,
                FromTeacherName = request.FromTeacher?.FullName ?? "Unknown",
                ToTeacherId = request.ToTeacherId,
                ToTeacherName = request.ToTeacher?.FullName ?? "Unknown",
                Reason = request.Reason,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            };
        }
    }
}

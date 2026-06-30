using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<IEnumerable<SubjectResponseDto>> GetByCenterIdAsync(int centerId)
        {
            var subjects = await _subjectRepository.GetByCenterIdAsync(centerId);
            return subjects.Select(MapToDto);
        }

        public async Task<SubjectResponseDto?> GetByIdAsync(int subjectId, int centerUserId)
        {
            var subject = await _subjectRepository.GetByIdWithMaterialsAsync(subjectId);
            if (subject == null) return null;

            // Chỉ trả về nếu trung tâm sở hữu môn học này
            if (subject.CenterId != centerUserId) return null;

            return MapToDto(subject);
        }

        public async Task<SubjectResponseDto> CreateAsync(SubjectCreateDto dto, int centerUserId)
        {
            // Kiểm tra mã môn học không trùng trong trung tâm
            var isDuplicate = await _subjectRepository.IsCodeDuplicateAsync(centerUserId, dto.SubjectCode.Trim());
            if (isDuplicate)
            {
                throw new InvalidOperationException($"Mã môn học '{dto.SubjectCode}' đã tồn tại trong trung tâm của bạn.");
            }

            var subject = new Subject
            {
                CenterId = centerUserId,
                SubjectCode = dto.SubjectCode.Trim().ToUpper(),
                SubjectName = dto.SubjectName.Trim(),
                Description = dto.Description?.Trim(),
                NumberOfSessions = dto.NumberOfSessions,
                CreatedAt = DateTime.Now
            };

            var created = await _subjectRepository.CreateAsync(subject);
            return MapToDto(created);
        }

        public async Task<bool> UpdateAsync(SubjectUpdateDto dto, int centerUserId)
        {
            var subject = await _subjectRepository.GetByIdAsync(dto.Id);
            if (subject == null) return false;

            // Chỉ cho phép Center chủ sở hữu cập nhật
            if (subject.CenterId != centerUserId) return false;

            // Kiểm tra mã môn học không trùng (ngoại trừ môn đang sửa)
            var isDuplicate = await _subjectRepository.IsCodeDuplicateAsync(centerUserId, dto.SubjectCode.Trim(), dto.Id);
            if (isDuplicate)
            {
                throw new InvalidOperationException($"Mã môn học '{dto.SubjectCode}' đã tồn tại trong trung tâm của bạn.");
            }

            subject.SubjectCode = dto.SubjectCode.Trim().ToUpper();
            subject.SubjectName = dto.SubjectName.Trim();
            subject.Description = dto.Description?.Trim();
            subject.NumberOfSessions = dto.NumberOfSessions;

            return await _subjectRepository.UpdateAsync(subject);
        }

        public async Task<bool> DeleteAsync(int subjectId, int centerUserId)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId);
            if (subject == null) return false;

            // Chỉ cho phép Center chủ sở hữu xóa
            if (subject.CenterId != centerUserId) return false;

            return await _subjectRepository.DeleteAsync(subjectId);
        }

        // Helper: Map Subject entity -> SubjectResponseDto
        private static SubjectResponseDto MapToDto(Subject s) => new()
        {
            Id = s.Id,
            CenterId = s.CenterId,
            SubjectCode = s.SubjectCode,
            SubjectName = s.SubjectName,
            Description = s.Description,
            NumberOfSessions = s.NumberOfSessions,
            CreatedAt = s.CreatedAt,
            MaterialCount = s.Materials?.Count ?? 0
        };
    }
}

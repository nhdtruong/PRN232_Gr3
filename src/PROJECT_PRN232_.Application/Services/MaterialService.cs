using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly INotificationService _notificationService;

        public MaterialService(
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            INotificationService notificationService)
        {
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _notificationService = notificationService;
        }

        // Xem danh sách học liệu của 1 buổi học
        public async Task<IEnumerable<MaterialResponseDto>> GetByLessonIdAsync(int lessonId)
        {
            var list = await _materialRepository.GetByLessonIdAsync(lessonId);
            return list.Select(MapToDto);
        }

        // Center upload tài liệu vào buổi học — TRIGGER NOTIFICATION cho phụ huynh
        public async Task<MaterialResponseDto?> CreateAsync(int lessonId, MaterialCreateDto dto, int centerUserId)
        {
            // 1. Kiểm tra Buổi học tồn tại và lấy kèm thông tin Lớp học để check CenterId
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return null;

            // 2. Bảo mật: Đảm bảo Center này sở hữu lớp học của buổi học
            if (lesson.Class.CenterId != centerUserId) return null;

            var material = new Material
            {
                LessonId = lessonId,
                Title = dto.Title,
                MaterialType = dto.MaterialType,
                FileURL = dto.FileURL,
                UploadedAt = DateTime.Now
            };

            var created = await _materialRepository.CreateAsync(material);
            return MapToDto(created);
        }

        // Center xóa tài liệu học tập
        public async Task<bool> DeleteAsync(int materialId, int centerUserId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null) return false;

            // Kiểm tra xem Center có quyền sở hữu lớp học chứa tài liệu này không
            var lesson = await _lessonRepository.GetLessonWithClassAsync(material.LessonId);
            if (lesson == null || lesson.Class.CenterId != centerUserId) return false;

            return await _materialRepository.DeleteAsync(materialId);
        }

        // Helper mapper
        private static MaterialResponseDto MapToDto(Material m) => new()
        {
            Id = m.Id,
            LessonId = m.LessonId,
            Title = m.Title,
            MaterialType = m.MaterialType,
            FileURL = m.FileURL,
            UploadedAt = m.UploadedAt
        };
    }
}

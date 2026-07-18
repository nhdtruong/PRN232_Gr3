using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly INotificationService _notificationService;
        private readonly IStudentRepository _studentRepository;

        public LessonService(
            ILessonRepository lessonRepository,
            INotificationService notificationService,
            IStudentRepository studentRepository)
        {
            _lessonRepository = lessonRepository;
            _notificationService = notificationService;
            _studentRepository = studentRepository;
        }

        public async Task<IEnumerable<LessonResponseDto>> GetLessonsByClassIdAsync(int classId)
        {
            var lessons = await _lessonRepository.GetByClassIdAsync(classId);
            return lessons.Select(MapToDto);
        }

        public async Task<LessonResponseDto?> GetByIdAsync(int lessonId)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return null;
            return MapToDto(lesson);
        }

        public async Task<LessonResponseDto> CreateAsync(LessonCreateDto dto, int teacherUserId)
        {
            var classObj = await _lessonRepository.GetClassByIdAsync(dto.ClassId);
            if (classObj == null)
            {
                throw new ArgumentException("Lớp học không tồn tại.");
            }
            if (classObj.TeacherId != teacherUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thêm buổi học cho lớp này.");
            }

            if (dto.RoomId.HasValue && dto.SlotId.HasValue)
            {
                var isCollided = await _lessonRepository.CheckCollisionAsync(dto.LessonDate, dto.SlotId.Value, dto.RoomId.Value);
                if (isCollided)
                {
                    throw new InvalidOperationException("Phòng học này đã có lớp khác sử dụng ở ca học và ngày được chọn.");
                }
            }

            var lesson = new Lesson
            {
                ClassId = dto.ClassId,
                Title = dto.Title,
                Description = dto.Description,
                LessonDate = dto.LessonDate,
                RoomId = dto.RoomId,
                SlotId = dto.SlotId,
                IsPublished = false
            };

            var created = await _lessonRepository.CreateAsync(lesson);
            return MapToDto(created);
        }

        public async Task<bool> UpdateAsync(LessonUpdateDto dto, int teacherUserId)
        {
            var existing = await _lessonRepository.GetLessonWithClassAsync(dto.Id);
            if (existing == null) return false;

            if (existing.Class.TeacherId != teacherUserId) return false;

            if (dto.RoomId.HasValue && dto.SlotId.HasValue)
            {
                var isCollided = await _lessonRepository.CheckCollisionAsync(dto.LessonDate, dto.SlotId.Value, dto.RoomId.Value, dto.Id);
                if (isCollided)
                {
                    throw new InvalidOperationException("Phòng học này đã có lớp khác sử dụng ở ca học và ngày được chọn.");
                }
            }

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.LessonDate = dto.LessonDate;
            existing.RoomId = dto.RoomId;
            existing.SlotId = dto.SlotId;

            return await _lessonRepository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int lessonId, int teacherUserId)
        {
            var existingLesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (existingLesson == null) return false;

            if (existingLesson.Class.TeacherId != teacherUserId) return false;

            return await _lessonRepository.DeleteAsync(lessonId);
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByStudentForParentAsync(int studentId, int parentUserId)
        {
            var isOwnChild = await _studentRepository.IsOwnChildAsync(studentId, parentUserId);
            if (!isOwnChild)
            {
                return Enumerable.Empty<LessonResponseDto>();
            }

            var lessons = await _lessonRepository.GetLessonsByStudentIdAsync(studentId);
            return lessons.Where(l => l.IsPublished).Select(MapToDto);
        }

        public async Task<bool> PublishAsync(int lessonId, int teacherUserId)
        {
            var lesson = await _lessonRepository.GetLessonWithMaterialsAsync(lessonId);
            if (lesson == null) return false;

            if (lesson.Class.TeacherId != teacherUserId) return false;

            bool isRebroadcast = lesson.IsPublished;

            await _lessonRepository.PublishLessonAsync(lessonId);

            var materialTitles = lesson.Materials.Select(m => m.Title).ToList();

            await _notificationService.NotifyPublishedLessonAsync(
                lesson.Id,
                lesson.ClassId,
                lesson.Class.ClassName,
                lesson.Title,
                lesson.LessonDate,
                materialTitles,
                isRebroadcast);

            return true;
        }

        private static LessonResponseDto MapToDto(Lesson l) => new()
        {
            Id = l.Id,
            ClassId = l.ClassId,
            ClassName = l.Class?.ClassName ?? string.Empty,
            Title = l.Title,
            Description = l.Description,
            LessonDate = l.LessonDate,
            IsPublished = l.IsPublished,
            RoomId = l.RoomId,
            RoomName = l.Room?.RoomName,
            SlotId = l.SlotId,
            SlotName = l.Slot?.SlotName,
            StartTime = l.Slot?.StartTime,
            EndTime = l.Slot?.EndTime
        };
    }
}

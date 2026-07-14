using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;

        public ClassService(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync()
        {
            var classes = await _classRepository.GetAllClassesAsync();
            return classes.Select(c => new ClassResponseDto
            {
                Id = c.Id,
                ClassName = c.ClassName,
                CenterId = c.CenterId,
                Status = c.Status,
                StudentCount = c.ClassStudents?.Count ?? 0,
                MaxCapacity = c.MaxCapacity,
                Subject = c.Subject,
                TotalLessons = c.TotalLessons,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher?.FullName
            });
        }

        public async Task<ClassResponseDto?> GetClassByIdAsync(int id)
        {
            var classEntity = await _classRepository.GetClassByIdAsync(id);
            if (classEntity == null) return null;

            return new ClassResponseDto
            {
                Id = classEntity.Id,
                ClassName = classEntity.ClassName,
                CenterId = classEntity.CenterId,
                Status = classEntity.Status,
                StudentCount = classEntity.ClassStudents?.Count ?? 0,
                MaxCapacity = classEntity.MaxCapacity,
                Subject = classEntity.Subject,
                TotalLessons = classEntity.TotalLessons,
                StartDate = classEntity.StartDate,
                EndDate = classEntity.EndDate,
                TeacherId = classEntity.TeacherId,
                TeacherName = classEntity.Teacher?.FullName
            };
        }

        public async Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto)
        {
            var classEntity = new Class
            {
                ClassName = dto.ClassName,
                CenterId = dto.CenterId,
                MaxCapacity = dto.MaxCapacity,
                Subject = dto.Subject,
                TotalLessons = dto.TotalLessons,
                TeacherId = dto.TeacherId
            };

            var createdClass = await _classRepository.AddClassAsync(classEntity);

            return new ClassResponseDto
            {
                Id = createdClass.Id,
                ClassName = createdClass.ClassName,
                CenterId = createdClass.CenterId,
                Status = createdClass.Status,
                StudentCount = 0,
                MaxCapacity = createdClass.MaxCapacity,
                Subject = createdClass.Subject,
                TotalLessons = createdClass.TotalLessons,
                StartDate = createdClass.StartDate,
                EndDate = createdClass.EndDate,
                TeacherId = createdClass.TeacherId
            };
        }

        public async Task<bool> UpdateClassAsync(ClassUpdateDto dto)
        {
            var classEntity = await _classRepository.GetClassByIdAsync(dto.Id);
            if (classEntity == null) return false;

            classEntity.ClassName = dto.ClassName;
            classEntity.CenterId = dto.CenterId;
            classEntity.MaxCapacity = dto.MaxCapacity;
            classEntity.Subject = dto.Subject;
            classEntity.TotalLessons = dto.TotalLessons;
            classEntity.TeacherId = dto.TeacherId;

            return await _classRepository.UpdateClassAsync(classEntity);
        }

        public async Task<bool> UpdateClassStatusAsync(int id, string status)
        {
            var classEntity = await _classRepository.GetClassByIdAsync(id);
            if (classEntity == null) return false;

            classEntity.Status = status;
            return await _classRepository.UpdateClassAsync(classEntity);
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            return await _classRepository.DeleteClassAsync(id);
        }
    }
}

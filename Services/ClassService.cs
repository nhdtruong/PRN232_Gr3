using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;

namespace PROJECT_PRN232_.Services
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
                CenterId = c.CenterId
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
                CenterId = classEntity.CenterId
            };
        }

        public async Task<ClassResponseDto> CreateClassAsync(ClassCreateDto dto)
        {
            var classEntity = new Class
            {
                ClassName = dto.ClassName,
                CenterId = dto.CenterId
            };

            var createdClass = await _classRepository.AddClassAsync(classEntity);

            return new ClassResponseDto
            {
                Id = createdClass.Id,
                ClassName = createdClass.ClassName,
                CenterId = createdClass.CenterId
            };
        }

        public async Task<bool> UpdateClassAsync(ClassUpdateDto dto)
        {
            var classEntity = new Class
            {
                Id = dto.Id,
                ClassName = dto.ClassName,
                CenterId = dto.CenterId
            };

            return await _classRepository.UpdateClassAsync(classEntity);
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            return await _classRepository.DeleteClassAsync(id);
        }
    }
}

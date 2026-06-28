using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Repositories;

namespace PROJECT_PRN232_.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly ILessonRepository _lessonRepository;

        public AssessmentService(IAssessmentRepository assessmentRepository, ILessonRepository lessonRepository)
        {
            _assessmentRepository = assessmentRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<IEnumerable<AssessmentResponseDto>> GetByLessonIdAsync(int lessonId, int? parentIdFilter = null)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return Enumerable.Empty<AssessmentResponseDto>();

            var enrolledIds = await _lessonRepository.GetEnrolledStudentIdsAsync(lesson.ClassId);
            var assessments = await _assessmentRepository.GetByLessonIdAsync(lessonId);

            return assessments
                .Where(a => enrolledIds.Contains(a.StudentId))
                .Where(a => !parentIdFilter.HasValue || a.Student.ParentId == parentIdFilter.Value)
                .Select(MapToDto);
        }

        public async Task<bool> SaveBulkAsync(int lessonId, LessonAssessmentBulkDto dto, int centerUserId)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null || lesson.Class.CenterId != centerUserId)
                return false;

            var enrolledIds = await _lessonRepository.GetEnrolledStudentIdsAsync(lesson.ClassId);
            if (!ValidateStudentIds(dto.Items.Select(i => i.StudentId), enrolledIds))
                return false;

            if (!ValidateScores(dto.Items))
                return false;

            var entities = dto.Items.Select(i => new Assessment
            {
                Id = i.Id ?? 0,
                StudentId = i.StudentId,
                LessonId = lessonId,
                Score = i.Score,
                TeacherComment = i.TeacherComment
            });

            await _assessmentRepository.UpsertBulkAsync(lessonId, entities);
            return true;
        }

        internal static bool ValidateScores(IEnumerable<AssessmentUpsertDto> items)
        {
            return items.All(i => !i.Score.HasValue || (i.Score >= 0 && i.Score <= 10));
        }

        private static AssessmentResponseDto MapToDto(Assessment a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student.FullName,
            LessonId = a.LessonId,
            Score = a.Score,
            TeacherComment = a.TeacherComment,
            DateAssessed = a.DateAssessed
        };

        private static bool ValidateStudentIds(IEnumerable<int> studentIds, HashSet<int> enrolledIds)
        {
            return studentIds.All(enrolledIds.Contains);
        }
    }
}

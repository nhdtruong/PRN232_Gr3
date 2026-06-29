using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Domain;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Repositories;

namespace PROJECT_PRN232_.Application.Services
{
    public class LessonRollCallService : ILessonRollCallService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly INotificationService _notificationService;

        public LessonRollCallService(
            ILessonRepository lessonRepository,
            IAttendanceRepository attendanceRepository,
            IAssessmentRepository assessmentRepository,
            INotificationService notificationService)
        {
            _lessonRepository = lessonRepository;
            _attendanceRepository = attendanceRepository;
            _assessmentRepository = assessmentRepository;
            _notificationService = notificationService;
        }

        public async Task<LessonRollCallResponseDto?> GetRollCallByLessonAsync(int lessonId, int? parentIdFilter = null)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null) return null;

            var rows = await _lessonRepository.GetRollCallDataAsync(lessonId, lesson.ClassId, parentIdFilter);

            return new LessonRollCallResponseDto
            {
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                ClassId = lesson.ClassId,
                ClassName = lesson.Class.ClassName,
                LessonDate = lesson.LessonDate,
                Rows = rows.Select(r => new LessonRollCallRowDto
                {
                    StudentId = r.Student.Id,
                    StudentName = r.Student.FullName,
                    ParentId = r.Student.ParentId,
                    AttendanceId = r.Attendance?.Id,
                    Status = r.Attendance?.Status ?? AttendanceStatus.Present,
                    Note = r.Attendance?.Note,
                    AssessmentId = r.Assessment?.Id,
                    Score = r.Assessment?.Score,
                    TeacherComment = r.Assessment?.TeacherComment
                }).ToList()
            };
        }

        public async Task<bool> SaveRollCallAsync(int lessonId, LessonRollCallBulkUpsertDto dto, int centerUserId)
        {
            var lesson = await _lessonRepository.GetLessonWithClassAsync(lessonId);
            if (lesson == null || lesson.Class.CenterId != centerUserId)
                return false;

            var enrolledIds = await _lessonRepository.GetEnrolledStudentIdsAsync(lesson.ClassId);
            var rowStudentIds = dto.Rows.Select(r => r.StudentId).ToList();

            if (rowStudentIds.Count == 0 || !rowStudentIds.All(enrolledIds.Contains))
                return false;

            if (!AssessmentService.ValidateScores(dto.Rows.Select(r => new AssessmentUpsertDto
            {
                StudentId = r.StudentId,
                Score = r.Score,
                TeacherComment = r.TeacherComment
            })))
                return false;

            var attendances = dto.Rows.Select(r => new Attendance
            {
                StudentId = r.StudentId,
                Status = r.Status,
                Note = r.Note
            });

            var assessments = dto.Rows.Select(r => new Assessment
            {
                StudentId = r.StudentId,
                Score = r.Score,
                TeacherComment = r.TeacherComment
            });

            await _attendanceRepository.UpsertBulkAsync(lessonId, attendances);
            await _assessmentRepository.UpsertBulkAsync(lessonId, assessments);

            await SendRollCallNotificationsAsync(lesson, dto.Rows);

            return true;
        }

        private async Task SendRollCallNotificationsAsync(Lesson lesson, List<LessonRollCallRowDto> rows)
        {
            if (rows == null || rows.Count == 0) return;

            var rollCallData = await _lessonRepository.GetRollCallDataAsync(lesson.Id, lesson.ClassId);
            var studentMap = rollCallData.ToDictionary(r => r.Student.Id, r => r.Student);

            foreach (var row in rows)
            {
                if (!studentMap.TryGetValue(row.StudentId, out var student)) continue;
                if (student.ParentId == 0) continue; // skip student with no parent

                await _notificationService.NotifyRollCallUpdatedAsync(
                    student.ParentId,
                    lesson.ClassId,
                    lesson.Class.ClassName,
                    lesson.Title,
                    student.FullName,
                    row.Status,
                    row.Note,
                    row.Score,
                    row.TeacherComment);
            }
        }
    }
}


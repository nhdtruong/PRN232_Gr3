using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface ILessonRepository
    {
        Task<Lesson?> GetLessonWithClassAsync(int lessonId);
        Task<HashSet<int>> GetEnrolledStudentIdsAsync(int classId);
        Task<IEnumerable<(Student Student, Attendance? Attendance, Assessment? Assessment)>> GetRollCallDataAsync(int lessonId, int classId, int? parentIdFilter = null);

        Task<IEnumerable<Lesson>> GetByClassIdAsync(int classId);
        Task<Lesson> CreateAsync(Lesson lesson);
        Task<bool> UpdateAsync(Lesson lesson);
        Task<bool> DeleteAsync(int lessonId);
        Task<Class?> GetClassByIdAsync(int classId);
        Task<IEnumerable<Lesson>> GetLessonsByStudentIdAsync(int studentId);
        Task<bool> PublishLessonAsync(int lessonId);
        Task<Lesson?> GetLessonWithMaterialsAsync(int lessonId);
    }
}

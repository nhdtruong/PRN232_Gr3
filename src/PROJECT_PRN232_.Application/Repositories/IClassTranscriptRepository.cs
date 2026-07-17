using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Application.Repositories
{
    public interface IClassTranscriptRepository
    {
        Task<IEnumerable<ClassTranscript>> GetByClassIdAsync(int classId);
        Task<ClassTranscript?> GetByStudentAndClassAsync(int studentId, int classId);
        Task<ClassTranscript> UpsertAsync(ClassTranscript transcript);
        Task<IEnumerable<ClassTranscript>> GetByStudentIdAsync(int studentId);
    }
}

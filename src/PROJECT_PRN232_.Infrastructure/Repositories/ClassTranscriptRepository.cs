using PROJECT_PRN232_.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class ClassTranscriptRepository : IClassTranscriptRepository
    {
        private readonly AppDbContext _context;

        public ClassTranscriptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassTranscript>> GetByClassIdAsync(int classId)
        {
            return await _context.ClassTranscripts
                .Include(ct => ct.Student)
                .Where(ct => ct.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassTranscript?> GetByStudentAndClassAsync(int studentId, int classId)
        {
            return await _context.ClassTranscripts
                .FirstOrDefaultAsync(ct => ct.StudentId == studentId && ct.ClassId == classId);
        }

        public async Task<ClassTranscript> UpsertAsync(ClassTranscript transcript)
        {
            var existing = await _context.ClassTranscripts
                .FirstOrDefaultAsync(ct => ct.StudentId == transcript.StudentId && ct.ClassId == transcript.ClassId);

            if (existing != null)
            {
                existing.MidTermScore = transcript.MidTermScore;
                existing.MidTermComment = transcript.MidTermComment;
                existing.FinalScore = transcript.FinalScore;
                existing.FinalComment = transcript.FinalComment;
                existing.AverageDailyScore = transcript.AverageDailyScore;
                existing.FinalScoreTotal = transcript.FinalScoreTotal;
                
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                await _context.ClassTranscripts.AddAsync(transcript);
                await _context.SaveChangesAsync();
                return transcript;
            }
        }

        public async Task<IEnumerable<ClassTranscript>> GetByStudentIdAsync(int studentId)
        {
            return await _context.ClassTranscripts
                .Include(ct => ct.Class)
                .Where(ct => ct.StudentId == studentId)
                .ToListAsync();
        }
    }
}

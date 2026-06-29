using PROJECT_PRN232_.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.Infrastructure.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Material>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Materials
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.UploadedAt)
                .ToListAsync();
        }

        public async Task<Material?> GetByIdAsync(int materialId)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == materialId);
        }

        public async Task<Material> CreateAsync(Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<bool> DeleteAsync(int materialId)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null) return false;

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

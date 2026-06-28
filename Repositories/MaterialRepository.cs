using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Data;
using PROJECT_PRN232_.Data.Entities;

namespace PROJECT_PRN232_.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<Material>> GetByLessonIdAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<Material?> GetByIdAsync(int materialId)
        {
            throw new NotImplementedException();
        }

        public Task<Material> CreateAsync(Material material)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int materialId)
        {
            throw new NotImplementedException();
        }
    }
}

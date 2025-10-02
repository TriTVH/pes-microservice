using Microsoft.EntityFrameworkCore;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories
{
    public class SyllabusRepository : ISyllabusRepository
    {
        private PES_APP_FULL_DBContext _context;

        public SyllabusRepository(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }

        public async Task<int> CreateSyllabusAsync(Syllabus syllabus)
        {
            _context.Syllabi.Add(syllabus);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Syllabus>> GetAllSyllabusAsync()
        {
            return await _context.Syllabi.ToListAsync();
        }
        public async Task<IEnumerable<Syllabus>> GetAllActiveSyllabusAsync()
        {
            return await _context.Syllabi.Where(x=>x.IsActive == true).ToListAsync();
        }
        public async Task<Syllabus?> GetSyllabusByIdAsync(int id)
        {
            return await _context.Syllabi.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Syllabus?> GetSyllabusByNameAsync(string name)
        {
            return await _context.Syllabi.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<int> UpdateSyllabusAsync(Syllabus syllabus)
        {
            _context.Syllabi.Update(syllabus);
            return await _context.SaveChangesAsync();
        }
        public async Task<bool> IsDuplicateNameAsync(string name, int id)
        {
            return await _context.Syllabi
                .AnyAsync(s => s.Name == name && s.Id != id);
        }

    }
}

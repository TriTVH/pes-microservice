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
    public class ClassesRepository : IClassRepository
    {
        private PES_APP_FULL_DBContext _context;

        public ClassesRepository(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }
        public async Task<int> CreateClassAsync(Class classes)
        {
            _context.Classes.Add(classes);
            return await _context.SaveChangesAsync();
        }

        public async Task<Class?> GetClassByYearAndSyllabusId(int year, int syllabusId)
        {
            return await _context.Classes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.AcademicYear == year && x.SyllabusId == syllabusId);
        }

        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _context.Classes
                 .Include(x => x.Schedules)
                   .ThenInclude(s => s.Activities)
                         .Where(x => x.TeacherId == teacherId).ToListAsync();
        }

    }
}

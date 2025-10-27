using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories.Repository
{
    public class ClassRepository : IClassRepository
    {
        private readonly pesContext _context;

        public ClassRepository(pesContext context)
        {
            _context = context;
        }

        // AI Support Methods
        public async Task<IEnumerable<Class>> GetClassesWithSchedulesAsync(int limit = 10)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .ThenInclude(s => s.Activities)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalClassesCountAsync()
        {
            return await _context.Classes.CountAsync();
        }

        public async Task<int> GetActiveClassesCountAsync()
        {
            return await _context.Classes.CountAsync(c => c.Status == "ACTIVE");
        }

        // AI Dynamic Query Methods
        public async Task<Class?> GetClassByNameAsync(string name)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .ThenInclude(s => s.Activities)
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Class>> GetClassesByStatusAsync(string status, int limit = 10)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .Where(c => c.Status == status)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId, int limit = 10)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .Where(c => c.TeacherId == teacherId)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm, int limit = 10)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .Where(c => c.Name.Contains(searchTerm))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassesByCapacityAsync(int minCapacity, int maxCapacity, int limit = 10)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .Where(c => c.NumberStudent >= minCapacity && c.NumberStudent <= maxCapacity)
                .Take(limit)
                .ToListAsync();
        }
    }
}

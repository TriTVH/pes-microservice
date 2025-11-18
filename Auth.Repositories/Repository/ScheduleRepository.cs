using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories.Repository
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly pesContext _context;

        public ScheduleRepository(pesContext context)
        {
            _context = context;
        }

        // AI Support Methods
        public async Task<IEnumerable<Schedule>> GetSchedulesWithDetailsAsync(int limit = 10)
        {
            return await _context.Schedules
                .Include(s => s.Classes)
                .Include(s => s.Activities)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalSchedulesCountAsync()
        {
            return await _context.Schedules.CountAsync();
        }
    }
}

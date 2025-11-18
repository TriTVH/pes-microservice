using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories.Repository
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly pesContext _context;

        public ActivityRepository(pesContext context)
        {
            _context = context;
        }

        // AI Support Methods
        public async Task<IEnumerable<Activity>> GetActivitiesWithDetailsAsync(int limit = 10)
        {
            return await _context.Activities
                .Include(a => a.Schedule)
                .ThenInclude(s => s.Classes)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            return await _context.Activities.CountAsync();
        }
    }
}

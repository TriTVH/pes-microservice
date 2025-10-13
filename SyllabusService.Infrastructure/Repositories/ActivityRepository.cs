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
    public class ActivityRepository : IActivityRepository
    {
        private PES_APP_FULL_DBContext _context;

        public ActivityRepository(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Activity>> GetActivitiesByScheduleIdAsync(int scheduleId)
        {
            return await _context.Activities.Where(x => x.ScheduleId == scheduleId).ToListAsync();
        }

        public async Task<IEnumerable<Activity>> GetActivitiesBetweenStartDateAndEndDate(List<int?> classIds, DateOnly weekStart, DateOnly weekEnd)
        {
            var activities = await _context.Activities
                .Include(a => a.Schedule)
                .Where(a => classIds.Contains(a.Schedule.ClassesId)
                && a.Date >= weekStart &&
                a.Date <= weekEnd)
                .ToListAsync();
           var ordered = activities
          .OrderBy(a => a.Date)
          .ThenBy(a => a.StartTime)
          .ToList();
            return ordered;
        }

 

    }
}

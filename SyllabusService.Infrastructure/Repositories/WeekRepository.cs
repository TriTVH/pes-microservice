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
    public class WeekRepository : IWeekRepository
    {
        private PES_APP_FULL_DBContext _context;

        public WeekRepository(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByClassIdAsync(int classId)
        {
            return await _context.Schedules.Where(x => x.ClassesId == classId).ToListAsync();
        }

    }
}

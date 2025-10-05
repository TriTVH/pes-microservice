using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Auth.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repository
{
    public class TeacherActionRepository : ITeacherActionRepository
    {
        private readonly pesContext _context;

        public TeacherActionRepository(pesContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<Class?> GetClassDetailAsync(int classId, int teacherId)
        {
            return await _context.Classes
                .Include(c => c.Schedules)
                .ThenInclude(s => s.Activities)
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByTeacherIdAsync(int teacherId)
        {
            return await _context.Schedules
                .Include(s => s.Classes)
                .Where(s => s.Classes.TeacherId == teacherId)
                .Include(s => s.Activities)
                .ToListAsync();
        }

        public async Task<IEnumerable<Activity>> GetActivitiesByScheduleIdAsync(int scheduleId)
        {
            return await _context.Activities
                .Where(a => a.ScheduleId == scheduleId)
                .ToListAsync();
        }
    }
}

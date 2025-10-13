using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface IActivityRepository
    {
        Task<IEnumerable<Activity>> GetActivitiesByScheduleIdAsync(int scheduleId);
        Task<IEnumerable<Activity>> GetActivitiesBetweenStartDateAndEndDate(List<int?> classIds, DateOnly weekStart, DateOnly weekEnd);
    }
}

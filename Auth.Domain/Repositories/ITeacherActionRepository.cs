using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
namespace Auth.Domain.Repositories
{
    public interface ITeacherActionRepository
    {
        Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId);
        Task<Class?> GetClassDetailAsync(int classId, int teacherId);
        Task<IEnumerable<Schedule>> GetSchedulesByTeacherIdAsync(int teacherId);
        Task<IEnumerable<Activity>> GetActivitiesByScheduleIdAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetWeeklyScheduleAsync(int teacherId, string weekName);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
namespace Auth.Application.Services.IServices
{
    public interface ITeacherActionService
    {
        Task<IEnumerable<Class>> GetClassesAsync(int teacherId);
        Task<Class?> GetClassDetailAsync(int classId, int teacherId);
        Task<IEnumerable<Schedule>> GetSchedulesAsync(int teacherId);
        Task<IEnumerable<Activity>> GetActivitiesByScheduleAsync(int scheduleId);
    }
}

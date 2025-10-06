using Auth.Application.DTOs.Teacher;
using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Auth.Application.Services.IServices
{
    public interface ITeacherActionService
    {
        Task<IEnumerable<Class>> GetClassesAsync(int teacherId);
        Task<Class?> GetClassDetailAsync(int classId, int teacherId);
        Task<IEnumerable<Schedule>> GetSchedulesAsync(int teacherId);
        Task<IEnumerable<Activity>> GetActivitiesByScheduleAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetWeeklyScheduleAsync(int teacherId, string weekName);

    }
}

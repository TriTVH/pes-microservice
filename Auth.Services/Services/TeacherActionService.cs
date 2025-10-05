using Auth.Application.Services.IServices;
using Auth.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
namespace Auth.Application.Services
{
    public class TeacherActionService : ITeacherActionService
    {
        private readonly ITeacherActionRepository _repository;

        public TeacherActionService(ITeacherActionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Class>> GetClassesAsync(int teacherId)
        {
            return await _repository.GetClassesByTeacherIdAsync(teacherId);
        }

        public async Task<Class?> GetClassDetailAsync(int classId, int teacherId)
        {
            return await _repository.GetClassDetailAsync(classId, teacherId);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesAsync(int teacherId)
        {
            return await _repository.GetSchedulesByTeacherIdAsync(teacherId);
        }

        public async Task<IEnumerable<Activity>> GetActivitiesByScheduleAsync(int scheduleId)
        {
            return await _repository.GetActivitiesByScheduleIdAsync(scheduleId);
        }
    }
}

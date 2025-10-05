using Auth.Application.DTOs.Teacher;
using Auth.Application.Services.IServices;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Auth.Application.Services
{
    public class TeacherActionService : ITeacherActionService
    {
        private readonly ITeacherActionRepository _repository;
        private readonly ILogger<TeacherActionService> _logger;

        public TeacherActionService(ITeacherActionRepository teacherRepository, ILogger<TeacherActionService> logger)
        {
            _repository = teacherRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<Schedule>> GetWeeklyScheduleAsync(int teacherId, string weekName)
        {
            return await _repository.GetWeeklyScheduleAsync(teacherId, weekName);
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

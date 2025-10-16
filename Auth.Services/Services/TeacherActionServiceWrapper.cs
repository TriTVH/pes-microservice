using Auth.Services.DTOs.Common;
using Auth.Application.Services.IServices;
using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services.Services
{
    public class TeacherActionServiceWrapper
    {
        private readonly ITeacherActionService _teacherActionService;

        public TeacherActionServiceWrapper(ITeacherActionService teacherActionService)
        {
            _teacherActionService = teacherActionService;
        }

        // === CLASS MANAGEMENT ===
        public async Task<ServiceResponse<IEnumerable<Class>>> GetClassesWithResponseAsync(int teacherId)
        {
            try
            {
                var result = await _teacherActionService.GetClassesAsync(teacherId);
                return ServiceResponse<IEnumerable<Class>>.Success("Classes retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Class>>.Error($"Failed to get classes: {ex.Message}", null, "GET_CLASSES_ERROR");
            }
        }

        public async Task<ServiceResponse<Class>> GetClassDetailWithResponseAsync(int classId, int teacherId)
        {
            try
            {
                var result = await _teacherActionService.GetClassDetailAsync(classId, teacherId);
                if (result == null)
                    return ServiceResponse<Class>.Error("Class not found", null, "CLASS_NOT_FOUND");
                
                return ServiceResponse<Class>.Success("Class detail retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<Class>.Error($"Failed to get class detail: {ex.Message}", null, "GET_CLASS_DETAIL_ERROR");
            }
        }

        // === SCHEDULE MANAGEMENT ===
        public async Task<ServiceResponse<IEnumerable<Schedule>>> GetSchedulesWithResponseAsync(int teacherId)
        {
            try
            {
                var result = await _teacherActionService.GetSchedulesAsync(teacherId);
                return ServiceResponse<IEnumerable<Schedule>>.Success("Schedules retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Schedule>>.Error($"Failed to get schedules: {ex.Message}", null, "GET_SCHEDULES_ERROR");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Schedule>>> GetWeeklyScheduleWithResponseAsync(int teacherId, string weekName)
        {
            try
            {
                var result = await _teacherActionService.GetWeeklyScheduleAsync(teacherId, weekName);
                return ServiceResponse<IEnumerable<Schedule>>.Success($"Weekly schedule for {weekName} retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Schedule>>.Error($"Failed to get weekly schedule: {ex.Message}", null, "GET_WEEKLY_SCHEDULE_ERROR");
            }
        }

        // === ACTIVITY MANAGEMENT ===
        public async Task<ServiceResponse<IEnumerable<Activity>>> GetActivitiesByScheduleWithResponseAsync(int scheduleId)
        {
            try
            {
                var result = await _teacherActionService.GetActivitiesByScheduleAsync(scheduleId);
                return ServiceResponse<IEnumerable<Activity>>.Success("Activities retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Activity>>.Error($"Failed to get activities: {ex.Message}", null, "GET_ACTIVITIES_ERROR");
            }
        }
    }
}

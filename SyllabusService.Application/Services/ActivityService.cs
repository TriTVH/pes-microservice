using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services
{
    public class ActivityService : IActivityService
    {
        private IActivityRepository _repo;

        public ActivityService(IActivityRepository repo)
        {
            _repo = repo;
        }

        public async Task<ResponseObject> GetAllActivitiesByScheduleId(int scheduleId)
        {
            var items = await _repo.GetActivitiesByScheduleIdAsync(scheduleId);
            var result = items.Select(a => new ActivityDTO()
            {
                Id = a.Id,
                Name = a.Name,
                Date = a.Date,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            });
            return new ResponseObject("ok","View all activities of schedule successfully", result);
        }

        public async Task<ResponseObject> GetActivitiesBetweenStartDateAndEndDate(GetActivitiesBetweenStartDateAndEndDateRequest request)
        {
            if (!request.classIds.Any())
            {
                return new ResponseObject("badRequest", "Class IDs cannot be empty or null.", null);
            }
            if (request.startWeek > request.endWeek)
            {
                return new ResponseObject("badRequest", "Start week cannot be later than end week.", null);
            }
            var items = await _repo.GetActivitiesBetweenStartDateAndEndDate(request.classIds, request.startWeek, request.endWeek);
            var result = items.Select( a => new ActivityDTO()
            {
                Id = a.Id,
                Name = a.Name,
                DayOfWeek = a.DayOfWeek,
                Date = a.Date,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            });
            return new ResponseObject("ok", "Get Activities Between Start Week And End Week Successfully", result);
        }

    }
}

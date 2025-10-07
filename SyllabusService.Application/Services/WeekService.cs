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
    public class WeekService : IWeekService
    {
        private IWeekRepository _repo;

        public WeekService(IWeekRepository repo)
        {
            _repo = repo;
        }

        public async Task<ResponseObject> GetScheduleByClassId(int classId)
        {
            var items = await _repo.GetSchedulesByClassIdAsync(classId);

            var result = items.Select(w => new WeekDTO()
            {
                Id = w.Id,
                WeekName = w.WeekName,
                StartDate = DateOnly.FromDateTime(w.StartDate.Value),
                EndDate = DateOnly.FromDateTime(w.EndDate.Value)
            });

            return new ResponseObject("ok", "Get schedules of class successfully", items);
        }

    }
}

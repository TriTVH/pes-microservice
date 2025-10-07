using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services.IServices
{
    public interface IWeekService
    {
        Task<ResponseObject> GetScheduleByClassId(int classId);
    }
}

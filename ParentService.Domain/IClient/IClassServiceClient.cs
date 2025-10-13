using ParentService.Application.DTOs;
using ParentService.Domain.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Domain.IClient
{
    public interface IClassServiceClient
    {
        Task<ResponseObjectFromAnotherClient> CheckClassesAvailabilityAsync(CheckClassRequest request);
        Task<ResponseObjectFromAnotherClient> GetAdmissionTermById(int id);
        Task<ResponseObjectFromAnotherClient> GetByClassId(int id);
        Task<ResponseObjectFromAnotherClient> GetClassesByIds(List<int> ids);
        Task<ResponseObjectFromAnotherClient> GetActivitiesBetweenStartDateAndEndDate(GetActivitiesBetweenStartDateAndEndDateRequest request);
        Task<ResponseObjectFromAnotherClient> GetActiveAdmissionTerm();
    }
}

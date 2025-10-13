using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Domain.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.Services.IServices
{
    public interface IStudentService
    {
        Task<ResponseObject> CreateStudentAsync(CreateNewStudentRequest request, int parentAccountId);
        Task<ResponseObject> GetStudentsAsync(int parentId);

        Task<ResponseObject> GetStudentByIdAsync(int id);

        Task<ResponseObject> GetActivitiesBetweenStartDateAndEndDate(int studentId, WeekRequest request);
    }
}

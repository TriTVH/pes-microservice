using SyllabusService.Application.DTOs.Request;
using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services.IServices
{
    public interface IClassesServices
    {
        Task<ResponseObject> CreateClass(CreateClassRequest request);
        Task<ResponseObject> GetClassesAfterDateInYearAsync(DateOnly endDate);
        Task<ResponseObject> GetAllClassesAsync();
        Task<ResponseObject> CheckClassesAvailability(CheckClassRequest request);
    }
}

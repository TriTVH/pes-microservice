using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.Services.IServices
{
    public interface IAdmissionFormService
    {
        Task<ResponseObject> CreateAdmissionFormAsync(CreateFormRequest request, int parentAccId);
        Task<ResponseObject> CheckClassesAvailabilityAsync(CheckClassRequest request);
        Task<ResponseObject> GetAdmissionFormsByParentAccountId(int parentAccountId);
        Task<ResponseObject> GetClassesByAdmissionFormId(int afId);
    }
}

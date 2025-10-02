using SyllabusService.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services.IServices
{
    public interface IAdmissionTermService
    {
        Task<ResponseObject> CreateAdmissionTermAsync(CreateAdmissionTermRequest request);
        Task<ResponseObject> GetAllAdmissionTermsAsync();
    }
}

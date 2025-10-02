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
    public interface ISyllabusService
    {
        Task<ResponseObject> CreateSyllabusAsync(CreateSyllabusRequest request);
        Task<ResponseObject> UpdateSyllabusAsync(UpdateSyllabusRequest request);
        Task<ResponseObject> GetAllSyllabusAsync();
        Task<ResponseObject> GetAllActiveSyllabusAsync();
    }
}

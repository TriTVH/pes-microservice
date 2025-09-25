using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;
using TermAdmissionManagement.Application.DTOs.Request;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Application.Services.IService
{
    public interface IAdmissionTermService
    {
        Task<ResponseObject> CreateAdmissionTerm(CreateAdmissionTermRequest request);

        Task<ResponseObject?> GetAdmissionTerms();

        Task<ResponseObject> UpdateAdmissionTermStatus(UpdateAdmissionTermStatusRequest request);

        Task<ResponseObject> UpdateAdmissionTermInfoAsync(UpdateAdmissionTermRequest request);

    }
}

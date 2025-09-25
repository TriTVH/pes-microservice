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
    public interface IAdmissionFormService
    {
        //Task<ResponseObject> GetAdmissionFormsAsync();
        Task<ResponseObject> CreateAdmissionForm(CreateAdmissionFormRequest admissionForm, int id);
    }
}

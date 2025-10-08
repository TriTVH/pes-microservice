using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Application.Services.IServices;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.Services
{
    public class AdmissionFormService : IAdmissionFormService
    {
        private IAdmissionFormRepo _admissionRepo;
        private IStudentRepo _studentRepo;

        public AdmissionFormService(IAdmissionFormRepo admissionFormRepo, IStudentRepo studentRepo)
        {
            _admissionRepo = admissionFormRepo;
            _studentRepo = studentRepo;
        }

        public async Task<ResponseObject> CreateAdmissionFormAsync(CreateFormRequestWithNewStudentRequest request, int parentAccId)
        {

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SE Asia Standard Time"
                : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var student = await _studentRepo.GetStudentAsyncById(request.StudentId);

            var admissionForm = new AdmissionForm()
            {
                Student = student,
                ParentAccountId = parentAccId,
                AdmissionTermId = request.AdmissionTermId,
                Status = "waiting_for_approve",
                SubmittedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };
            await _admissionRepo.CreateAdmissionFormAsync(admissionForm);

            var admissionFormClasses = request.ClassIds.Select(classId => new AdmissionFormClass
            {
                ClassId = classId,
                AdmissionFormId = admissionForm.Id 
            }).ToList();
            await _admissionRepo.CreateAdmissionFormClassesAsync(admissionFormClasses);

            return new ResponseObject("ok","Create Admission Form Successfully", null);

        }

   
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;
using TermAdmissionManagement.Application.DTOs.Request;
using TermAdmissionManagement.Application.DTOs.Response;
using TermAdmissionManagement.Application.Services.IService;
using TermAdmissionManagement.Infrastructure.Entities;
using TermAdmissionManagement.Infrastructure.Repositories.IRepository;

namespace TermAdmissionManagement.Application.Services
{
    public class AdmissionFormService : IAdmissionFormService
    {
        private IAdmissionFormRepository _repo;
        public AdmissionFormService(IAdmissionFormRepository repo) 
        { 
            _repo = repo;
        }

        public async Task<ResponseObject> CreateAdmissionForm(CreateAdmissionFormRequest request, int parent_account_id)
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var vietNamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            AdmissionForm admissionFormPendingAlreadyInSystem = await _repo.findByTermItemIdAndStatusAndChild(request.TermItemId, "pending", request.StudentId);
             AdmissionForm admissionFormProcessingAlreadyInSystem = await _repo.findByTermItemIdAndStatusAndChild(request.TermItemId, "processing", request.StudentId);

            if (admissionFormPendingAlreadyInSystem != null || admissionFormProcessingAlreadyInSystem != null)
            {
                return new ResponseObject("badRequest", "The system found that you have admission form for this child in same year need to be checked before creating new form", null);
            }

            AdmissionForm admission = new AdmissionForm()
            {
                ChildCharacteristicsFormImg = request.ChildCharacteristicsFormImg,
                CommitmentImg = request.CommitmentImg,
                HouseholdRegistrationAddress = request.HouseholdRegistrationAddress,
                Note = request.Note,
                Status = "pending",
                SubmittedDate = vietNamNow,
                ParentAccountId = parent_account_id,
                TermItemId = request.TermItemId,
                StudentId = request.StudentId,
                PaymentExpiryDate = vietNamNow.AddDays(1)
            };
            await _repo.CreateAdmissionForm(admission);
            return new ResponseObject("ok", "Create admission form successfully", null);
        }

        public async Task<ResponseObject> GetAdmissionFormsAsync()
        {
            var items = await _repo.GetAllAsync();

            var result = items.Select(x => new AdmissionFormDto()
            {
                Id = x.Id,
                ApprovedDate = x.ApprovedDate,
                CancelReason = x.CancelReason,
                ChildCharacteristicsFormImg = x.ChildCharacteristicsFormImg,
                CommitmentImg = x.CommitmentImg,
                HouseholdRegistrationAddress = x.HouseholdRegistrationAddress,
                Note = x.Note,
                SubmittedDate = x.SubmittedDate,
                termItemGrade = x.TermItem.Grade

            });
            
            return new ResponseObject("ok", "Get All Admission form successfully", result);
        }


    }
}

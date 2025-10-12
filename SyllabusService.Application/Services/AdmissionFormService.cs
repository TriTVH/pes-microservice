using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services
{
    public class AdmissionFormService : IAdmissionFormService
    {
        private IAdmissionFormRepo _admissionFormRepo;
        private IParentClient _parentClient;
        private IAuthClient _authClient;
        public AdmissionFormService(IAdmissionFormRepo admissionFormRepo, IParentClient parentClient, IAuthClient authClient)
        {
            _admissionFormRepo = admissionFormRepo;
            _parentClient = parentClient;
            _authClient = authClient;
        }
        public async Task<ResponseObject> GetAdmissionFormsByAdmissionTermIdAsync(int admissionTermId)
        {
            var items = await _admissionFormRepo.GetAdmissionFormsByAdmissionTermIdAsync(admissionTermId);

            var result = new List<AdmissionFormDto>();

            foreach (var af in items)
            {

                var studentResult = await _parentClient.GetStudentDtoById(af.StudentId);
                var parentAccountResult = await _authClient.GetParentProfileDto(af.ParentAccountId);

                StudentDTO? student = null;
                if (studentResult?.Data is JsonElement element)
                {
                    student = element.Deserialize<StudentDTO>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }


                result.Add(new AdmissionFormDto
                {
                    Id = af.Id,
                    Student = student,
                    ParentAccount = parentAccountResult,
                    SubmittedDate = af.SubmittedDate,
                    ApprovedDate = af.ApprovedDate,
                    CancelReason = af.CancelReason,
                    Status = af.Status,
                });
            }

            return new ResponseObject("ok", "Get All Admission Forms Successfully", result);
        }

        public async Task<ResponseObject> ChangeStatusOfAdmissionForm(UpdateAdmissionFormStatusRequest request) 
        {

            var form = await _admissionFormRepo.GetAdmissionFormByIdAsync(request.Id);

            if (form == null)
            {
                return new ResponseObject("notFound", $"Admission form with ID {request.Id} not found.", null);
            }

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "SE Asia Standard Time"
    : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var action = request.Action?.Trim().ToLower();

            switch (action)
            {
                case "approve":
                    form.Status = "waiting_for_payment";
                    form.ApprovedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                    break;

                case "reject":
                    form.Status = "rejected";
                    form.CancelReason = string.IsNullOrWhiteSpace(request.CancelReason)
                        ? "Rejected by educational manager."
                        : request.CancelReason;
                    break;

                default:
                    return new ResponseObject("badRequest", "Invalid action. Only 'Approve' or 'Reject' are allowed.", null);
            }

            await _admissionFormRepo.UpdateAdmissionFormAsync(form);

            return new ResponseObject("ok", $"Admission form has been {action}d successfully.", null);
        }

    }
}

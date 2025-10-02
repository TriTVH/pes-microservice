using Microsoft.IdentityModel.Tokens;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.Application.Services
{
    public class AdmissionTermService : IAdmissionTermService
    {
        private IAdmissionTermRepo _admissionTermRepo;
        private IClassRepository _classRepo;
        public AdmissionTermService(IAdmissionTermRepo admissionTermRepo,
            IClassRepository classRepo)
        {
            _admissionTermRepo = admissionTermRepo;
            _classRepo = classRepo;
        }

        public async Task<ResponseObject> CreateAdmissionTermAsync(CreateAdmissionTermRequest request)
        {
            string error = ValidateCreateAdmissionTerm(request);
            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }
            var existingClasses = await _classRepo.GetExistingClassIdsAsync(request.classIds);

            var existingClassIds = existingClasses.Select(c => c.Id).ToList();

            var invalidClassIds = request.classIds.Except(existingClassIds).ToList();

            if (invalidClassIds.Any())
                return new ResponseObject("conflict", $"The following class IDs were recently deleted: {string.Join(", ", invalidClassIds)}. Please refresh list and try again", null);

            var overlappingTerm = await _admissionTermRepo.GetOverlappingTermAsync(request.startDate, request.endDate);
            if (overlappingTerm != null)
            {
                return new ResponseObject("conflict",
                    $"The selected dates overlap with an existing term from {overlappingTerm.StartDate:yyyy-MM-dd} to {overlappingTerm.EndDate:yyyy-MM-dd}.", null);
            }
            var newTerm = new AdmissionTerm
            {
                StartDate = request.startDate,
                EndDate = request.endDate,
                AcdemicYear = request.endDate.Year,
                NumberOfClasses = existingClasses.Count(),
                MaxNumberRegistration = existingClasses.Count * 30,
                CurrentRegisteredStudents = 0,
                Classes = existingClasses,
                Status = "inactive"
            };
            await _admissionTermRepo.CreateAdmissionTermAsync(newTerm);
            return new ResponseObject("ok", "Create admission term successfully", null);
        }

        public async Task<ResponseObject> GetAllAdmissionTermsAsync()
        {
            var items = await _admissionTermRepo.GetAdmissionTermsAsync();
            var result = items.Select(item => new AdmissionTermDto(
                item.Id,
                item.AcdemicYear,
                item.MaxNumberRegistration,
                item.CurrentRegisteredStudents,
                item.NumberOfClasses,
                item.StartDate,
                item.EndDate,
                item.Status,
                item.Classes.Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NumberOfWeeks = c.NumberOfWeeks,
                    NumberStudent = c.NumberStudent,
                    AcademicYear = c.AcademicYear,
                    Status = c.Status
                }).ToList()
              )).ToList();

            return new ResponseObject("ok", "Get all admission terms successfully", result);
        }

        private string ValidateCreateAdmissionTerm(CreateAdmissionTermRequest request)
        {
            if (request.startDate < DateTime.UtcNow)
                return "Start date cannot be in the past.";

            if (request.startDate >= request.endDate)
                return "Start date must be earlier than end date.";

            if (request.classIds == null || !request.classIds.Any())
                return "At least one class ID must be provided.";
            return "";
        }
    }
}

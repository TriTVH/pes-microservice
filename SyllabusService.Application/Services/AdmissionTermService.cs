using Microsoft.IdentityModel.Tokens;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public async Task<ResponseObject> UpdateAdmissionTermStatusByAction(UpdateAdmissionTermActionRequest request)
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                 ? "SE Asia Standard Time"
                 : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var term = await _admissionTermRepo.GetAdmissionTermByIdAsync(request.Id);

            if (term == null)
                return new ResponseObject("notFound", "Admission term not found.", null);

            var vietNamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            switch (request.Action.ToLower())
            {
                case "start":
                    if (term.Status != "inactive")
                        return new ResponseObject("badRequest", "Only inactive terms can be started.", null);

                    var overlap = await _admissionTermRepo.GetOverlappingTermAsyncExceptId(vietNamNow, term.EndDate, term.Id);

                    if (overlap != null)
                    {
                        return new ResponseObject("conflict",
                            $"Start time overlaps with another term from {overlap.StartDate:yyyy-MM-dd} to {overlap.EndDate:yyyy-MM-dd}.", null);
                    }

                    term.Status = "active";
                    term.StartDate = vietNamNow;
                    break;

                case "end":
                    if (term.Status != "active")
                        return new ResponseObject("badRequest", "Only active terms can be ended.", null);
                    term.Status = "blocked";
                    term.EndDate = vietNamNow;
                    break;

                default:
                    return new ResponseObject("badRequest", "Invalid action. Must be 'start' or 'end'.", null);
            }

            await _admissionTermRepo.UpdateAdmissionTermAsync(term);
            return new ResponseObject("ok", $"Admission term {request.Action}ed successfully.", null);

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
                    StartDate = c.StartDate,
                    Cost = c.Syllabus.Cost,
                    Status = c.Status
                }).ToList()
              )).ToList();

            return new ResponseObject("ok", "Get all admission terms successfully", result);
        }

        public async Task<ResponseObject> GetActiveAdmissionTermAsync()
        {
            var item = await _admissionTermRepo.GetActiveAdmissionTerm();

            if (item == null)
            {
                return new ResponseObject("notFound", "There is currently no active admission term available", null);
            }

            var result = new AdmissionTermDto(
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
                    StartDate = c.StartDate,
                    Cost = c.Syllabus.Cost,
                    Status = c.Status,
                    PatternActivitiesDTO = c.PatternActivities.Select(pa => new PatternActivityDto()
                    {
                        DayOfWeek = pa.DayOfWeek,
                        StartTime = pa.StartTime,
                        EndTime = pa.EndTime
                    }).ToList()
                }).ToList());

            return new ResponseObject("ok", "Get active admission terms successfully", result);
        }

        public async Task<ResponseObject> GetAdmissionTermById(int id)
        {
            var item = await _admissionTermRepo.GetAdmissionTermByIdAsync(id);

            if(item == null)
            {
                return new ResponseObject("notFound", "Admission term not found or be deleted", null);
            }

            var result = new AdmissionTermDto(id, item.AcdemicYear, item.MaxNumberRegistration, item.CurrentRegisteredStudents, item.NumberOfClasses, item.StartDate, item.EndDate, item.Status, null);
            return new ResponseObject("ok", "View admission term by id successfully", result);
        }

        public async Task<ResponseObject> GetComboBoxAdmissionTermsAsync()
        {
            var items = await _admissionTermRepo.GetPrioritizedAdmissionTermsAsync();
            var result = items.Select(at => new ComboItemAdmissionTerm()
            {
                Id = at.Id,
                Name = $"{DateOnly.FromDateTime(at.StartDate):dd-MM-yyyy} to {DateOnly.FromDateTime(at.EndDate):dd-MM-yyyy}"
            }).ToList();
            return new ResponseObject("ok", "Get Combo Box admission term successfully", result);
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

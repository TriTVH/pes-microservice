using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
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
    public class AdmissionTermService : IAdmissionTermService
    {
        private IAdmissionTermRepository _admissionTermRepo;
       
        public AdmissionTermService(IAdmissionTermRepository admissionTermRepository,
            ITermItemRepository termItemRepository) 
        {
            _admissionTermRepo = admissionTermRepository;
           
        }
    
        public async Task<ResponseObject> CreateAdmissionTerm(CreateAdmissionTermRequest request)
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            if (request.startDateTime < DateTime.UtcNow.Date)
            {
                return new ResponseObject("badRequest","Start date cannot be in the past.", null);
            }

            if (request.endDateTime <= request.startDateTime)
            {
                return new ResponseObject("badRequest", "Please ensure end date is after start date.", null);
            }

            var startDateVietNam = TimeZoneInfo.ConvertTimeFromUtc(request.startDateTime, vietnamTimeZone);

            var endDateVietnam = TimeZoneInfo.ConvertTimeFromUtc(request.endDateTime, vietnamTimeZone);

            if(request.termItems.Count <= 0)
            {
                return new ResponseObject("badRequest", "Please add at least one grade to create admission term", null);
            }

            // Validate TermItems
            var allowedGrades = new[] { "BUD", "LEAF", "SEED" };

            foreach (var item in request.termItems)
            {
                if (!allowedGrades.Contains(item.grade))
                    return new ResponseObject("badRequest", $"Invalid grade '{item.grade}'. Must be one of: BUD, LEAF, SEED.", null);

                if (item.expectedClasses <= 0)
                    return new ResponseObject("badRequest", "Expected Classes must be greater than 0.", null);

                
            }
            
            if(await _admissionTermRepo.GetByYear(endDateVietnam.Year) != null)
            {
                return new ResponseObject("conflict", $"An admission term for the year {endDateVietnam.Year} already exists.", null);
            }

            // Passed validation → proceed to mapping
            var admissionTerm = new AdmissionTerm
            {
                Name = "Admission term for the year " + endDateVietnam.Year,
                StartDate = startDateVietNam,
                EndDate = endDateVietnam,
                Year = endDateVietnam.Year,
                TermItems = request.termItems.Select(item => new TermItem
                {
                    ExpectedClasses = item.expectedClasses,
                    CurrentRegisteredStudents = 0,
                    MaxNumberRegistration = item.expectedClasses * 30,
                    Grade = item.grade,
                    Status = "awaiting",
                }).ToList()
            };

            await _admissionTermRepo.CreateAdmissionTermAsync(admissionTerm);

            return new ResponseObject("ok", "Admission term created successfully.", null);
        }



        public async Task<ResponseObject?> GetAdmissionTerms()
        {
            var items = await _admissionTermRepo.GetAdmissionTermsAsync();
            var result = items?.Select(term => new AdmissionTermDto
            {
                Id = term.Id,
                Name = term.Name,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                Year = term.Year,
                TermItems = term.TermItems.Select(t => new TermItemDTO
                {
                    Id = t.Id,
                    Grade = t.Grade,       // tuỳ entity TermItem của bạn
                    ExpectedClasses = t.ExpectedClasses,
                    MaxNumberRegistration = t.MaxNumberRegistration,
                    Status = t.Status
                }).ToList()
            }).ToList();
            return new ResponseObject("ok", "View all admission terms successfully", result);
        }

        public Task<ResponseObject?> UpdateAdmissionTermStatus(UpdateAdmissionTermStatus request)
        {
            throw new NotImplementedException();
        }
    }
}

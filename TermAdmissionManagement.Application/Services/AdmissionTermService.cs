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
        private ITermItemRepository _termItemRepository;
        public AdmissionTermService(IAdmissionTermRepository admissionTermRepository, ITermItemRepository termItemRepository
          ) 
        {
            _admissionTermRepo = admissionTermRepository;
            _termItemRepository = termItemRepository;
        }
    
        public async Task<ResponseObject> CreateAdmissionTerm(CreateAdmissionTermRequest request)
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            if (request.year < DateTime.UtcNow.Year)
            {
                return new ResponseObject("badRequest", "Year cannot be in the past.", null);
            }

            if (request.startDateTime < DateTime.UtcNow.Date)
            {
                return new ResponseObject("badRequest","Start date cannot be in the past.", null);
            }

            if (request.endDateTime <= request.startDateTime)
            {
                return new ResponseObject("badRequest", "Please ensure end date is after start date.", null);
            }
            if (request.endDateTime.Year != request.year)
            {
                return new ResponseObject("badRequest", "End date must be within the same year as the admission term.", null);
            }

            if (request.startDateTime.Year != request.year)
            {
                return new ResponseObject("badRequest", "Start date must be within the same year as the admission term.", null);
            }

            if (request.termItems.Count <= 0)
            {
                return new ResponseObject("badRequest", "Please add at least one grade to create admission term", null);
            }
            if (request.termItems.Count > 3)
            {
                return new ResponseObject("badRequest", "Exceed three allowed grades for creating admission term", null);
            }
            // Validate TermItems
            var allowedGrades = new[] { "BUD", "LEAF", "SEED" };

            foreach (var item in request.termItems)
            {
                if (!allowedGrades.Contains(item.grade))
                    return new ResponseObject("badRequest", $"Invalid grade '{item.grade}'. Must be one of: BUD, LEAF, SEED.", null);

                if (item.expectedClasses <= 0)
                    return new ResponseObject("badRequest", "Expected Classes must be greater than 0.", null);

      

                if (await _admissionTermRepo.GetByYearAndGrade(request.year, item.grade) != null)
                {
                    return new ResponseObject("conflict", $"An admission term of {item.grade} for the year {request.year} already exists.", null);
                }
            }
            var startDateVietNam = TimeZoneInfo.ConvertTimeFromUtc(request.startDateTime, vietnamTimeZone);

            var endDateVietnam = TimeZoneInfo.ConvertTimeFromUtc(request.endDateTime, vietnamTimeZone);

            // Passed validation → proceed to mapping
            foreach (var item in request.termItems)
            {
                AdmissionTerm admissionTerm = new AdmissionTerm()
                {
                    Name = $"Admission term for {item.grade} of the year {request.year}",
                    Year = request.year,
                    Grade = item.grade
                };
                TermItem termItem = new TermItem()
                {
                    ExpectedClasses = item.expectedClasses,
                    Status = "inactive",
                    MaxNumberRegistration = item.expectedClasses * 25,
                    StartDate = startDateVietNam,
                    EndDate = endDateVietnam,
                    Version = 1,
                    AdmissionTerm = admissionTerm
                };
                _termItemRepository.CreateTermItemAsync(termItem);
            }



                return new ResponseObject("ok", "Admission terms created successfully.", null);

        }



        public async Task<ResponseObject?> GetAdmissionTerms()
        {
            var items = await _admissionTermRepo.GetAdmissionTermsAsync();
            var result = items?.Select(term => new AdmissionTermDto
            {
                Id = term.Id,
                Name = term.Name,
                Grade = term.Grade,
                Year = term.Year,
                TermItems = term.TermItems.Select(t => new TermItemDTO
                {
                    Id = t.Id,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Version = t.Version,
                    ExpectedClasses = t.ExpectedClasses,
                    MaxNumberRegistration = t.MaxNumberRegistration,
                    Status = t.Status,
                    CurrentRegisteredStudents = t.CurrentRegisteredStudents,
                }).ToList()
            }).ToList();
            return new ResponseObject("ok", "View all admission terms successfully", result);
        }

        public async Task<ResponseObject> UpdateTermInfoRequest(UpdateTermInfoRequest request)
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
               ? "SE Asia Standard Time"
               : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);


          
            if (request.startDateTime < DateTime.UtcNow.Date)
            {
                return new ResponseObject("badRequest", "Start date cannot be in the past.", null);
            }

            if (request.endDateTime <= request.startDateTime)
            {
                return new ResponseObject("badRequest", "Please ensure end date is after start date.", null);
            }

            var startDateVietNam = TimeZoneInfo.ConvertTimeFromUtc(request.startDateTime, vietnamTimeZone);

            var endDateVietnam = TimeZoneInfo.ConvertTimeFromUtc(request.endDateTime, vietnamTimeZone);

            if (request.expectedClasses <= 0)
                return new ResponseObject("badRequest", "Expected Classes must be greater than 0.", null);


            var existingInSystem = await _termItemRepository.GetByIdsAsync(request.id);

            if (existingInSystem == null)
            {
                return new ResponseObject("notFound", "Admission term item not found or be deleted", null);
            }

            if (existingInSystem.Status.ToLower().Equals("active") || existingInSystem.Status.ToLower().Equals("block"))
            {
                return new ResponseObject("badRequest", "Cannot update this admission term item because it is already active or blocked.", null);
            }

            if (request.endDateTime.Year != existingInSystem.AdmissionTerm.Year || request.startDateTime.Year != existingInSystem.AdmissionTerm.Year)
            {
                return new ResponseObject("badRequest", "End date or start date must be within the same year of the admission term.", null);
            }

            if (!existingInSystem.IsCurrent)
            {
                return new ResponseObject("conflict", "Only the current admission term item can be updated.", null);
            }

           

            existingInSystem.StartDate = startDateVietNam;
            existingInSystem.EndDate = endDateVietnam;
            existingInSystem.ExpectedClasses = request.expectedClasses;

            await _termItemRepository.UpdateTermItem(existingInSystem);

            return new ResponseObject("ok", "Update admission term successfully", null);
        }

        


        //public async Task<ResponseObject?> UpdateAdmissionTermStatus(UpdateAdmissionTermStatusRequest request)
        //{
        //    var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        //            ? "SE Asia Standard Time"
        //            : "Asia/Ho_Chi_Minh";

        //    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        //    var vietNamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        //    var admissionTermInSystem = await _admissionTermRepo.GetByIdWithItemsAsync(request.id);
        //   if(admissionTermInSystem == null)
        //    {
        //        return new ResponseObject("notFound", "Admission term not found or be deleted", null);
        //    }
        //    if (request.action.ToLower().Equals("start"))
        //    {

        //        admissionTermInSystem.StartDate = vietNamNow;
        //        foreach (TermItem termItem in admissionTermInSystem.TermItems)
        //        {
        //            termItem.Status = "processing";
        //        }
        //        return new ResponseObject("ok", "Update Admission Term status 'processing' successfully", null);
        //    }
        //    if (request.action.ToLower().Equals("complete"))
        //    {
        //        admissionTermInSystem.EndDate = vietNamNow;
        //        foreach (TermItem termItem in admissionTermInSystem.TermItems)
        //        {
        //            termItem.Status = "done";
        //        }
        //        return new ResponseObject("ok", "Update Admission Term status 'done' successfully", null);
        //    }
        //    return new ResponseObject("badRequest", "Invalid action! Must be one of actions: start, complete", null);
        //}
    }
}

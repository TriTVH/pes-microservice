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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyllabusService.Application.Services
{
    public class SyllabusServ : ISyllabusService
    {
        private ISyllabusRepository _syllabusRepo;
        public SyllabusServ(ISyllabusRepository syllabusRepository)
        {
            _syllabusRepo = syllabusRepository;
        }

        public async Task<ResponseObject> CreateSyllabusAsync(CreateSyllabusRequest request)
        {
            string error = validateCreateSyllabus(request);

            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }

            var existingSyllabus = await _syllabusRepo.GetSyllabusByNameAsync(request.name);

            if (existingSyllabus != null)
            {
                return new ResponseObject("conflict", "Syllabus name already exists.", null);
            }

            Syllabus syllabus = new()
            {
                Name = request.name,
                Description = request.description,
                Cost = request.cost,
                HoursOfSyllabus = request.hoursOfSyllabus
            };
           syllabus.IsActive = true;

            await _syllabusRepo.CreateSyllabusAsync(syllabus);
            return new ResponseObject("ok", "Create Syllabus Successfully", null);
        }

        public async Task<ResponseObject> GetAllActiveSyllabusAsync()
        {
            var items = await _syllabusRepo.GetAllActiveSyllabusAsync();
            var result = items.Select(s => new SyllabusDTO()
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Cost = s.Cost,
                HoursOfSyllabus = s.HoursOfSyllabus,
                IsActive = s.IsActive ? "true" : "false"
            });

            return new ResponseObject("ok", "Get All Active syllabuses successfully", result);
        }

        public async Task<ResponseObject> GetAllSyllabusAsync()
        {
            var items = await _syllabusRepo.GetAllSyllabusAsync();
            var result = items.Select(s => new SyllabusDTO()
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Cost = s.Cost,
                HoursOfSyllabus = s.HoursOfSyllabus,
                IsActive = s.IsActive ? "true" : "false"
            });
            
            return new ResponseObject("ok", "Get All syllabuses successfully", result);
        }

        public async Task<ResponseObject> UpdateSyllabusAsync(UpdateSyllabusRequest request)
        {
            string error = validateUpdateSyllabus(request);

            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }
            var existingInSystem = await _syllabusRepo.GetSyllabusByIdAsync(request.id);
            if (existingInSystem == null)
            {
                return new ResponseObject("notFound", "Syllabus doesn't found or be deleted", null);
            }
            if (await _syllabusRepo.IsDuplicateNameAsync(request.name, request.id))
            {
                return new ResponseObject("conflict", "Syllabus name already exists for another record", null);
            }
            existingInSystem.Name = request.name;
            existingInSystem.Cost = request.cost;
            existingInSystem.Description = request.description;
            if (request.isActive.ToLower().Equals("true"))
            {
                existingInSystem.IsActive = true;
            }
            else existingInSystem.IsActive = false;

            await _syllabusRepo.UpdateSyllabusAsync(existingInSystem);

            return new ResponseObject("ok", "Update Syllabus Successfully", null);
        }

        private string validateCreateSyllabus(CreateSyllabusRequest request)
        {
            if (string.IsNullOrEmpty(request.name))
            {
                return "Syllabus name must not be empty.";
            }
            if (string.IsNullOrEmpty(request.description))
            {
                return "Syllabus description must not be empty.";
            }
            if (request.cost < 100000)
            {
                return "Cost of syllabus must be greater than 100,000 VND";
            }
            if (request.hoursOfSyllabus <= 10 || request.hoursOfSyllabus > 40)
                return "Hours of syllabus must be greater than 10 and not greater than 40.";
           
          
            return "";
        }

        private string validateUpdateSyllabus(UpdateSyllabusRequest request)
        {

            if (string.IsNullOrEmpty(request.name))
            {
                return "Syllabus name must not be empty.";
            }
            if (string.IsNullOrEmpty(request.description))
            {
                return "Syllabus description must not be empty.";
            }
            if (request.cost < 100000)
            {
                return "Cost of syllabus must be greater than 100,000 VND";
            }
            if (request.hoursOfSyllabus <= 10 || request.hoursOfSyllabus > 40)
                return "Hours of syllabus must be greater than 10 and not greater than 40.";
            if (string.IsNullOrEmpty(request.isActive))
            {
                return "Is Active must not be empty.";
            }
            if (!new[] { "true", "false" }.Contains(request.isActive.ToLower()))
                return "Is Active must be either 'true' or 'false'.";
            return "";
        }

    }
}

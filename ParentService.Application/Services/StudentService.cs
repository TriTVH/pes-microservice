using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Application.DTOs.Response;
using ParentService.Application.Services.IServices;
using ParentService.Domain.DTOs.Request;
using ParentService.Domain.DTOs.Response;
using ParentService.Domain.IClient;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParentService.Application.Services
{
    public class StudentService : IStudentService
    {
        private IStudentRepo _studentRepo;
        private IClassServiceClient _classServiceClient;
        public StudentService(IStudentRepo studentRepo, IClassServiceClient classServiceClient)
        {
            _studentRepo = studentRepo;
            _classServiceClient = classServiceClient;
        }

        public async Task<ResponseObject> CreateStudentAsync(CreateNewStudentRequest request, int parentAccountId)
        {
            string error = ValidateCreateStudentAsync(request);
            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }

            if (await _studentRepo.ExistByStudentNameAndParentId(request.Name, parentAccountId))
            {
                return new ResponseObject("conflict", "A children with this name already exists under your account.", null);
            }

                Student student = new()
            {
                Name = request.Name,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                PlaceOfBirth = request.PlaceOfBirth,
                BirthCertificateImg = request.BirthCertificateImg,
                ProfileImage = request.ProfileImage,
                HouseholdRegistrationImg = request.HouseholdRegistrationImg.Trim(),
                IsStudent = false,
                ParentAccId = parentAccountId
            };

            await _studentRepo.CreateStudentAsync(student);
            return new ResponseObject("ok", "Create children Successfully", null);
        }

        public async Task<ResponseObject> GetStudentsAsync(int parentId)
        {
            var items = await _studentRepo.GetStudentsAsyncByParentAccId(parentId);

            var result = items.Select(s => new StudentDTO()
            {
                Id = s.Id,
                Name = s.Name,
                DateOfBirth = s.DateOfBirth,
                PlaceOfBirth = s.PlaceOfBirth,
                Gender = s.Gender,
                BirthCertificateImg = s.BirthCertificateImg,
                ProfileImage = s.ProfileImage,
                HouseholdRegistrationImg = s.HouseholdRegistrationImg,
                IsStudent = s.IsStudent
            });

            return new ResponseObject("ok", "View all your childrens successfully", result);

        }

        private string ValidateCreateStudentAsync(CreateNewStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name is required.";

            if (request.DateOfBirth is null)
                return "Date of birth is required.";

            var age = CalculateAge(request.DateOfBirth.Value);

            if (age < 3 || age > 5)
                return "Student age must be between 3 and 5 years old.";

            if (string.IsNullOrWhiteSpace(request.Gender))
                return "Gender is required.";
            else if (!new[] { "Male", "Female" }.Contains(request.Gender, StringComparer.OrdinalIgnoreCase))
                return "Gender must be Male, Female.";

            if (string.IsNullOrWhiteSpace(request.PlaceOfBirth))
                return "Place of birth is required.";

            if (string.IsNullOrWhiteSpace(request.ProfileImage))
                return "Profile image is required." ;

            if (string.IsNullOrWhiteSpace(request.HouseholdRegistrationImg))
                return "Household registration image is required.";

            if (string.IsNullOrWhiteSpace(request.BirthCertificateImg))
                return "Birth certificate image is required.";

            return "";
        }
        private int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dateOfBirth.Year;

            // Nếu chưa đến sinh nhật trong năm nay thì trừ đi 1
            if (dateOfBirth > today.AddYears(-age))
                age--;

            return age;
        }

        public async Task<ResponseObject> GetStudentByIdAsync(int id)
        {
            var item = await _studentRepo.GetStudentAsyncById(id);
            if(item == null)
            {
                return new ResponseObject("notFound", "Student not found or be deleted", null);
            }
            var result = new StudentDTO()
            {
                Id = id,
                BirthCertificateImg = item.BirthCertificateImg,
                HouseholdRegistrationImg = item.HouseholdRegistrationImg,
                ProfileImage = item.ProfileImage,
                Name = item.Name,
                DateOfBirth = item.DateOfBirth,
                PlaceOfBirth = item.PlaceOfBirth,
                Gender = item.Gender,
                IsStudent = item.IsStudent
            };
            return new ResponseObject("ok", "View student by id successfully", result);
        }

        public async Task<ResponseObject> GetActivitiesBetweenStartDateAndEndDate(int studentId, WeekRequest request)
        {
            var classIds = await _studentRepo.GetClassIdsByStudentIdAsync(studentId);

            if (request.startWeek > request.endWeek)
            {
                return new ResponseObject("badRequest", "Start week cannot be later than end week.", null);
            }

            if (!classIds.Any())
            {
                return new ResponseObject("ok", "Get Activities between start week and end week successfully", new List<ActivityDTO>());
            }
            
            GetActivitiesBetweenStartDateAndEndDateRequest getActivitiesRequest = new GetActivitiesBetweenStartDateAndEndDateRequest();
            getActivitiesRequest.classIds = classIds;
            getActivitiesRequest.startWeek = request.startWeek;
            getActivitiesRequest.endWeek = request.endWeek;

            var activitiesResult = await _classServiceClient.GetActivitiesBetweenStartDateAndEndDate(getActivitiesRequest);

            var activitiesDTO = ((JsonElement)activitiesResult.Data).Deserialize<List<ActivityDTO>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

             return new ResponseObject("ok", "Get Activities between start week and end week successfully", activitiesDTO);

        }

        public async Task<ResponseObject> UpdateStudentAsync(int parentAccountId, UpdateStudentRequest request)
        {
            var student = await _studentRepo.GetStudentAsyncById(request.Id);

            if (student == null)
            {
                return new ResponseObject("notFound", "Student not found.", null);
            }

            string error = ValidateUpdateStudent(request);

            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }

            if (student.IsStudent)
            {
                return new ResponseObject("conflict", "Cannot update information of an enrolled student.", null);
            }

      
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != student.Name)
            {
                var duplicateName = await _studentRepo.CheckDuplicateNameStudentOfParent(parentAccountId, request.Name, request.Id);

                if (duplicateName)
                    return new ResponseObject("conflict",$"A student named '{request.Name}' already exists for this parent account.", null);
            }
            student.Name = request.Name;
            student.DateOfBirth = request.DateOfBirth;
            student.Gender = request.Gender;
            student.PlaceOfBirth = request.PlaceOfBirth;
            student.ProfileImage = request.ProfileImage;
            student.BirthCertificateImg = request.BirthCertificateImg;
            student.HouseholdRegistrationImg = request.HouseholdRegistrationImg;

            await _studentRepo.UpdateStudentAsync(student);

            return new ResponseObject("ok", "Student updated successfully.", null);
        }

        public async Task<ResponseObject> GetClassesByStudentId(int studentId)
        {
            var student = await _studentRepo.GetStudentAsyncById(studentId);

            if (student == null)
            {
                return new ResponseObject("notFound", "Student not found.", null);
            }

            var classIds = await _studentRepo.GetClassIdsByStudentIdAsync(studentId);

            var classesResult = await _classServiceClient.GetClassesByIds(classIds);

            var classes = ((JsonElement)classesResult.Data).Deserialize<List<ClassDto>>(
             new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ResponseObject("ok", "Get classes of student successfully", classes);
        

        }

        private string ValidateUpdateStudent(UpdateStudentRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (request.Name.Length < 2 || request.Name.Length > 50)
                   return "Name must be between 2 and 50 characters.";
            }

            if (request.DateOfBirth is null)
                return "Date of birth is required.";

            var age = CalculateAge(request.DateOfBirth.Value);

            if (age < 3 || age > 5)
                return "Student age must be between 3 and 5 years old.";

            if (string.IsNullOrWhiteSpace(request.Gender))
                return "Gender is required.";
            else if (!new[] { "Male", "Female" }.Contains(request.Gender, StringComparer.OrdinalIgnoreCase))
                return "Gender must be Male, Female.";

            if (string.IsNullOrWhiteSpace(request.PlaceOfBirth))
                return "Place of birth is required.";

            if (string.IsNullOrWhiteSpace(request.ProfileImage))
                return "Profile image is required.";

            if (string.IsNullOrWhiteSpace(request.HouseholdRegistrationImg))
                return "Household registration image is required.";

            if (string.IsNullOrWhiteSpace(request.BirthCertificateImg))
                return "Birth certificate image is required.";
            return "";
        }

    }
}

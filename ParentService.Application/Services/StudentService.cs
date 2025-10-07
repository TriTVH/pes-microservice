using Microsoft.IdentityModel.Tokens;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Application.DTOs.Response;
using ParentService.Application.Services.IServices;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParentService.Application.Services
{
    public class StudentService : IStudentService
    {
        private IStudentRepo _studentRepo;
        public StudentService(IStudentRepo studentRepo)
        {
            _studentRepo = studentRepo;
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

    }
}

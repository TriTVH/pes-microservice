using Microsoft.SqlServer.Server;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Application.DTOs.Response;
using ParentService.Application.Services.IServices;
using ParentService.Domain.DTOs.Request;
using ParentService.Domain.DTOs.Response;
using ParentService.Domain.IClient;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories;
using ParentService.Infrastructure.Repositories.IRepositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParentService.Application.Services
{
    public class AdmissionFormService : IAdmissionFormService
    {
        private IAdmissionFormRepo _admissionRepo;
        private IStudentRepo _studentRepo;
        private IClassServiceClient _classServiceClient;
        public AdmissionFormService(IAdmissionFormRepo admissionFormRepo, IStudentRepo studentRepo, IClassServiceClient classServiceClient)
        {
            _admissionRepo = admissionFormRepo;
            _studentRepo = studentRepo;
            _classServiceClient = classServiceClient;
        }

        public async Task<ResponseObject> CreateAdmissionFormAsync(CreateFormRequest request, int parentAccId)
        {

            Student student = await _studentRepo.GetStudentAsyncById(request.StudentId);

            var fullClasses = new List<string>();

            if (student == null)
            {
                return new ResponseObject("notFound", "Student not found or be deleted", null);
            }

            if (request.ClassIds.Count == 0)
            {
                return new ResponseObject("badRequest", "Admission form must contain at least one class.", null);
            }

            foreach (var classId in request.ClassIds)
            {

                var result = await _classServiceClient.GetByClassId(classId);


                var classDto = ((JsonElement)result.Data).Deserialize<ClassDto>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (classDto.NumberStudent >= 30)
                {
                    fullClasses.Add(classDto.Name);
                    continue;
                }

                var formStatus = await _admissionRepo.GetStudentAdmissionFormStatusAsync(request.StudentId, classId);

                if (formStatus is not null)
                {
                    if (formStatus.Equals("waiting_for_approve", StringComparison.OrdinalIgnoreCase))
                    {
                        return new ResponseObject(
                            "conflict",
                            $"Your admission form for class name {classDto.Name} is currently under review.",
                            null
                        );
                    }

                    if (formStatus.Equals("waiting_for_payment", StringComparison.OrdinalIgnoreCase) || formStatus.Equals("payment_in_progress", StringComparison.OrdinalIgnoreCase) )
                    {
                        return new ResponseObject(
                            "conflict",
                            $"Your admission form for class name {classDto.Name} has been sended and is waiting for payment.",
                           null
                        );
                    }

                    if (formStatus.Equals("done", StringComparison.OrdinalIgnoreCase))
                    {
                        return new ResponseObject(
                            "conflict",
                            $"You have already been accepted into class name {classDto.Name}.",
                            null
                        );
                    }
                }
            }

            if (fullClasses.Any())
            {
                string fullList = string.Join(", ", fullClasses);
                return new ResponseObject("badRequest", $"The following class(es) are already full: {fullList}. Please remove these items to countinue saving admission form", null);
            }

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SE Asia Standard Time"
                : "Asia/Ho_Chi_Minh";



            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var admissionForm = new AdmissionForm()
            {
                Student = student,
                ParentAccountId = parentAccId,
                AdmissionTermId = request.AdmissionTermId,
                SubmittedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            if (student.IsStudent)
            {
                admissionForm.Status = "waiting_for_payment";
            }
            else
            {
                admissionForm.Status = "waiting_for_approve";
            }

            await _admissionRepo.CreateAdmissionFormAsync(admissionForm);

            var admissionFormClasses = request.ClassIds.Select(classId => new AdmissionFormClass
            {
                ClassId = classId,
                AdmissionFormId = admissionForm.Id 
            }).ToList();

            await _admissionRepo.CreateAdmissionFormClassesAsync(admissionFormClasses);

            return new ResponseObject("ok","Create Admission Form Successfully", null);

        }

        public async Task<ResponseObject> DeleteAdmissionForm(int afId)
        {
            var form = await _admissionRepo.GetAdmissionFormByIdAsync(afId);

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found or has been deleted.", null);
            }

            if (form.Status.Equals("payment_in_progress"))
            {
                return new ResponseObject(
          "conflict",
          $"Cannot delete this admission form because payment is currently in progress.",
          null
      );

            }

            await _admissionRepo.DeleteAdmissionForm(form);

            return new ResponseObject("ok", "Delete Admission Form Successfully", null);
        }

        public async Task<ResponseObject> RemoveClassesFromAdmissionForm(RemoveClassesFromAdmissionFormRequest request)
        {
            if (!request.ClassIds.Any())
            {
                return new ResponseObject("badRequest", "Class IDs list cannot be empty.", null);
            }
            var form = await _admissionRepo.GetAdmissionFormByIdAsync(request.AdmissionFormId);

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found or has been deleted.", null);
            }
            if (form.Status.Equals("done", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseObject("badRequest", "This admission form has already been completed and cannot be modified.", null);
            }
            var classesToRemove = form.AdmissionFormClasses
           .Where(c => request.ClassIds.Contains(c.ClassId))
           .ToList();
            if (!classesToRemove.Any())
            {
                return new ResponseObject("notFound", "No matching classes found in this admission form.", null);
            }
            foreach (var classToRemove in classesToRemove)
            {
                form.AdmissionFormClasses.Remove(classToRemove);
            }

            int remainingClassesCount = form.AdmissionFormClasses.Count - classesToRemove.Count;
            if (remainingClassesCount <= 0)
            {
                return new ResponseObject("badRequest", "Cannot remove all classes. An admission form must have at least one class.", null);
            }

            // 🔹 Save changes
            await _admissionRepo.UpdateAdmissionFormAsync(form);

            return new ResponseObject("ok", $"Removed {classesToRemove.Count} class(es) from the admission form successfully.", null);
        }

        public async Task<ResponseObject> CheckClassesAvailabilityAsync(CheckClassRequest request)
        {

            var activeTemResponse = await _classServiceClient.GetActiveAdmissionTerm();

            if(activeTemResponse == null)
            {
                return new ResponseObject("notFound", "Active admission term not found or over due", null);
            }

            var activeTerm = ((JsonElement)activeTemResponse.Data).Deserialize<AdmissionTermDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (activeTerm == null)
            {
                return new ResponseObject("notFound", "Active admission term not found or over due", null);
            }

            var validClassIds = activeTerm.ClassDtos.Select(c => c.Id).ToHashSet();

            if (!validClassIds.Contains(request.CurrentClassId))
            {
                return new ResponseObject("conflict", $"This class is not part of the active admission term.", null);
            }

            if (!request.CheckedClassIds.Any())
            {
                List<int> classIds = new List<int>();

                classIds.Add(request.CurrentClassId);

                var classListResult = await _classServiceClient.GetClassesByIds(classIds);

                var classList = ((JsonElement)classListResult.Data).Deserialize<List<ClassDto>>(
                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (!classList.Any())
                {
                    return new ResponseObject("notFound", "No class information found.", null);
                }

                return new ResponseObject("ok", "No schedule conflicts detected.", classList);
            }

            var clasResult = await _classServiceClient.GetByClassId(request.CurrentClassId);

            var classDto = ((JsonElement)clasResult.Data).Deserialize<ClassDto>(
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
              );

            if(classDto.NumberStudent >= 30)
            {
                return new ResponseObject("conflict", $"{classDto.Name} is full of students", null);
            }

            var checkedClassesResult = await _classServiceClient.GetClassesByIds(request.CheckedClassIds);

            var checkedClasses = ((JsonElement)checkedClassesResult.Data).Deserialize<List<ClassDto>>(
             new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
             );

            var formStatus = await _admissionRepo.GetStudentAdmissionFormStatusAsync(request.StudentId, request.CurrentClassId);

            if (formStatus is not null)
            {
                if (formStatus.Equals("waiting_for_approve", StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseObject(
                        "conflict",
                        $"Your admission form for class name {classDto.Name} is currently under review.",
                            checkedClasses
                    );
                }

                if (formStatus.Equals("waiting_for_payment", StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseObject(
                        "conflict",
                        $"Your admission form for class name {classDto.Name} has been sended and is waiting for payment.",
                         checkedClasses
                    );
                }

                if (formStatus.Equals("done", StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseObject(
                        "conflict",
                        $"You have already been accepted into class name {classDto.Name}.",
                        checkedClasses
                    );
                }
            }

            var result = await _classServiceClient.CheckClassesAvailabilityAsync(request);

            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return new ResponseObject(result.StatusResponseCode, result.Message, checkedClasses);
            }

            if (result.StatusResponseCode.Equals("conflict"))
            {
                return new ResponseObject(result.StatusResponseCode, result.Message, checkedClasses);
            }

            var checkedClassIdsAfter = ((JsonElement)result.Data).Deserialize<List<int>>(
                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
             );

            var checkedClassesAfterResult = await _classServiceClient.GetClassesByIds(checkedClassIdsAfter);

            var checkedClassesAfter = ((JsonElement)checkedClassesAfterResult.Data).Deserialize<List<ClassDto>>(
             new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
         );

            return new ResponseObject("ok", result.Message, checkedClassesAfter);
        }

        public async Task<ResponseObject> GetAdmissionFormsByParentAccountId(int parentAccountId)
        {
            var items = await _admissionRepo.GetAdmissionFormsByParentAccountIdAsync(parentAccountId);
            var result = new List<AdmissionFormDto>();
            foreach (var af in items)
            {
                var admissionTermResult = await _classServiceClient.GetAdmissionTermById(af.AdmissionTermId);

                var admissionTerm = ((JsonElement)admissionTermResult.Data).Deserialize<AdmissionTermDto>(
             new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

              

                var student = await _studentRepo.GetStudentAsyncById(af.StudentId);

                result.Add(new AdmissionFormDto
                {
                    Id = af.Id,
                    Student = new StudentDTO()
                    {
                        Id = af.Id,
                        Name = student.Name,
                        Gender = student.Gender,
                        PlaceOfBirth = student.PlaceOfBirth,
                        DateOfBirth = student.DateOfBirth,
                        IsStudent = student.IsStudent,
                        BirthCertificateImg = student.BirthCertificateImg,
                        HouseholdRegistrationImg = student.HouseholdRegistrationImg,
                        ProfileImage = student.ProfileImage
                    },
                    AdmissionTermStartDate = admissionTerm.StartDate,
                    AdmissionTermEndDate = admissionTerm.EndDate,
                    SubmittedDate = af.SubmittedDate,
                    ApprovedDate = af.ApprovedDate,
                    CancelReason = af.CancelReason,
                    Status = af.Status,
                });
            }

            return new ResponseObject("ok", "View All admission forms of parent successfully", result);
        }

        public async Task<ResponseObject> GetClassesByAdmissionFormId(int afId) 
        {
            var form = await _admissionRepo.GetAdmissionFormByIdAsync(afId);

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found or be deleted", null);
            }

            var classIds = await _admissionRepo.GetClassIdsByAdmissionFormId(afId);

            var classesResult = await _classServiceClient.GetClassesByIds(classIds);

            var classes = ((JsonElement)classesResult.Data).Deserialize<List<ClassDto>>(
           new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ResponseObject("ok", "View all classes by admission form id successfully", classes);
        }

    }
}

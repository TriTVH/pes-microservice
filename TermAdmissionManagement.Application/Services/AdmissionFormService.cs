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
        //private IAuthClient _authClient;
        public AdmissionFormService(IAdmissionFormRepository repo) 
        { 
            _repo = repo;
            //_authClient = authClient;
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

        //public async Task<ResponseObject> GetAdmissionFormsAsync()
        //{



        //    //var items = await _repo.GetAllAsync();

        //    //var result = items.Select(x => new AdmissionFormDto()
        //    //{
        //    //    Id = x.Id,
        //    //    ApprovedDate = x.ApprovedDate,
        //    //    CancelReason = x.CancelReason,
        //    //    ChildCharacteristicsFormImg = x.ChildCharacteristicsFormImg,
        //    //    CommitmentImg = x.CommitmentImg,
        //    //    HouseholdRegistrationAddress = x.HouseholdRegistrationAddress,
        //    //    Note = x.Note,
        //    //    SubmittedDate = x.SubmittedDate,
        //    //    termItemGrade = x.TermItem.Grade,

        //    //});

        //    var items = await _repo.GetAllAsync(); // List<AdmissionForm>

        //    // 2. Tập unique parent ids
        //    var parentIds = items
        //        .Where(x => x.ParentAccountId > 0)
        //        .Select(x => x.ParentAccountId)
        //        .Distinct()
        //        .ToList();

        //    // 3. Khởi tạo các task gọi auth service song song
        //    var parentTasks = parentIds.ToDictionary(
        //        id => id,
        //        id =>_authClient.GetParentAccountInfoAsync(id) // Task<ResponseObjectFromAnotherClient<AccountDto>?>
        //    );

        //    await Task.WhenAll(parentTasks.Values);

        //    // 3. Collect kết quả
        //    var parentResponses = new Dictionary<int?, ParentAccountDto?>();

        //    foreach (var kv in parentTasks)
        //    {
        //        var resp = await kv.Value; // await task
        //        parentResponses[kv.Key] = resp?.Data; // chỉ lấy Data, tránh null ref
        //    }

        //    // 4. Map về AdmissionFormDto
        //    var result = items.Select(x => new AdmissionFormDto
        //    {
        //        Id = x.Id,
        //        ApprovedDate = x.ApprovedDate,
        //        CancelReason = x.CancelReason,
        //        ChildCharacteristicsFormImg = x.ChildCharacteristicsFormImg,
        //        CommitmentImg = x.CommitmentImg,
        //        HouseholdRegistrationAddress = x.HouseholdRegistrationAddress,
        //        Note = x.Note,
        //        SubmittedDate = x.SubmittedDate,
        //        termItemGrade = x.TermItem.Grade,
        //        ParentEmail = parentResponses.TryGetValue(x.ParentAccountId, out var parent)
        //            ? parent?.Email
        //            : null,
        //        ParentName = parentResponses.TryGetValue(x.ParentAccountId, out var parentName)
        //            ? parentName?.Name
        //            : null,
        //        ParentPhoneNumber = parentResponses.TryGetValue(x.ParentAccountId, out var parentPhonenumber)
        //            ? parentPhonenumber?.Phone
        //            : null,
        //    });

        //    return new ResponseObject("ok", "Get All Admission form successfully", result);
        //}


    }
}

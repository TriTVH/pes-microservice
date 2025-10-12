using Microsoft.Extensions.Configuration;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Response;
using ParentService.Application.Libraries;
using ParentService.Application.Services.IServices;
using ParentService.Domain.IClient;
using ParentService.Infrastructure.Repositories.IRepositories;
using SyllabusService.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParentService.Application.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IAdmissionFormRepo _admissionFormRepo;
        private readonly IClassServiceClient _classServiceClient;

        public VnPayService(IConfiguration configuration, IAdmissionFormRepo admissionFormRepo, IClassServiceClient classServiceClient) 
        {
            _configuration = configuration;
            _admissionFormRepo = admissionFormRepo;
            _classServiceClient = classServiceClient;
        }
        
        public async Task<ResponseObject> GetPaymentUrl(string ipAdress, int formId)
        {

            var form = await _admissionFormRepo.GetAdmissionFormByIdAsync(formId);

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found or be deleted", null);
            }

            var admissionTermResult = await _classServiceClient.GetAdmissionTermById(form.AdmissionTermId);

            var admissionTerm = ((JsonElement)admissionTermResult.Data).Deserialize<AdmissionTermDto>(
         new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var classIds = await _admissionFormRepo.GetClassIdsByAdmissionFormId(formId);

            var classesResult = await _classServiceClient.GetClassesByIds(classIds);

            var classes = ((JsonElement)classesResult.Data).Deserialize<List<ClassDto>>(
           new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            int? totalCost = 0;

            foreach (var clas in classes)
            {
                totalCost = totalCost + clas.Cost;
            }

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "SE Asia Standard Time"
            : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var tick = timeNow.Ticks.ToString();

            var defaultExpire = timeNow.AddMinutes(10);
            DateTime expireTime;

            var termEnd = TimeZoneInfo.ConvertTimeFromUtc(admissionTerm.EndDate.ToUniversalTime(), vietnamTimeZone);

            var remainingMinutes = (termEnd - timeNow).TotalMinutes;
            expireTime = remainingMinutes < 10 ? termEnd : defaultExpire;


            var pay = new VnPayLibrary();

            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", (totalCost * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_ExpireDate", expireTime.ToString("yyyyMMddHHmmss")); 
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", ipAdress);
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{form.Id}");
            pay.AddRequestData("vnp_OrderType", "online");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
               pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return new ResponseObject("ok", paymentUrl, null);
        }

    }
}

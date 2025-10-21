using Azure;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Response;
using ParentService.Application.Libraries;
using ParentService.Application.Services.IServices;
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
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IAdmissionFormRepo _admissionFormRepo;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IStudentRepo _studentRepo;
        private readonly IClassServiceClient _classServiceClient;
        private readonly IRequestClient<PaymentSuccessEvent> _requestClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public VnPayService(IConfiguration configuration, IAdmissionFormRepo admissionFormRepo, IClassServiceClient classServiceClient, IRequestClient<PaymentSuccessEvent> requestClient, ITransactionRepo transactionRepo, IStudentRepo studentRepo, IPublishEndpoint publishEndpoint)
        {
            
            _configuration = configuration;
            _admissionFormRepo = admissionFormRepo;
            _transactionRepo = transactionRepo;
            _classServiceClient = classServiceClient;
            _requestClient = requestClient;
            _studentRepo = studentRepo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ResponseObject> GetPaymentUrl(string ipAdress, int formId)
        {

            var form = await _admissionFormRepo.GetAdmissionFormByIdAsync(formId);

            var fullClasses = new List<string>();

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found or be deleted", null);
            }

            if (form.Status.Equals("done"))
            {
                return new ResponseObject("conflict", "This admission form has already been completed.", null);
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
                
                if (clas.NumberStudent >= 30)
                {
                    fullClasses.Add(clas.Name);
                    continue;
                }

            }
            if (fullClasses.Any())
            {
                string fullList = string.Join(", ", fullClasses);
                return new ResponseObject("badRequest", $"The following class(es) are already full: {fullList}. Please remove these items to countinue paying", null);
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

            form.Status = "payment_in_progress";
            await _admissionFormRepo.UpdateAdmissionFormAsync(form);

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                await _publishEndpoint.Publish(new PaymentTimeoutEvent
                {
                    AdmissionFormId = form.Id,
                    CreatedAt = DateTime.UtcNow
                });
            });

            Console.WriteLine($"[Scheduler] Published PaymentTimeoutEvent for formId={form.Id}");

            return new ResponseObject("ok", paymentUrl, null);
        }

        public async Task<ResponseObject> ConfirmPaymentUrl(
       string vnp_Amount,
       string vnp_OrderInfo,
       string vnp_PayDate,
       string vnp_TransactionStatus,
       string vnp_TxnRef)
        {

            if (!int.TryParse(vnp_OrderInfo, out var formId))
            {
                return new ResponseObject("badRequest", "Invalid order info (formId).", null);
            }

            var form = await _admissionFormRepo.GetAdmissionFormByIdAsync(formId);

            if (form == null)
            {
                return new ResponseObject("notFound", "Admission form not found.", null);
            }

            DateTime? payDate = null;
            if (!string.IsNullOrEmpty(vnp_PayDate) &&
                DateTime.TryParseExact(vnp_PayDate, "yyyyMMddHHmmss",
                    null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                payDate = parsedDate;
            }

            int amount = 0;

            if (int.TryParse(vnp_Amount, out var rawAmount))
            {
                amount = rawAmount / 100;
            }

            if (vnp_TransactionStatus == "00") // 00 = Thành công
            {

                form.Status = "done";

                await _admissionFormRepo.UpdateAdmissionFormAsync(form);

                var classIds = await _admissionFormRepo.GetClassIdsByAdmissionFormId(formId);

                var response = await _requestClient.GetResponse<ClassProcessResultEvent>(
              new PaymentSuccessEvent
              {
                  AdmissionFormId = formId,
                  ClassIds = classIds,
                  Amount = amount,
                  TxnRef = vnp_TxnRef,
                  PayDate = payDate
              },
              timeout: RequestTimeout.After(s: 90));
                var result = response.Message;

                var classesResponse = await _classServiceClient.GetClassesByIds(classIds);
                var classes = ((JsonElement)classesResponse.Data).Deserialize<List<ClassDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var successClasses = classes
                    .Where(c => result.SuccessfulClassIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.Cost })
                    .ToList();

                var fullClasses = classes
                    .Where(c => result.FailedClassIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.Cost })
                    .ToList();

                var successTotal = successClasses.Sum(c => c.Cost ?? 0);
                var refundTotal = fullClasses.Sum(c => c.Cost ?? 0);

                if (successTotal > 0)
                {
                   

                    if (!form.Student.IsStudent)
                    {
                        form.Student.IsStudent = true;
                        await _studentRepo.UpdateStudentAsync(form.Student);
                    }

                    foreach (var cls in successClasses)
                    {
                        var studentClass = new StudentClass
                        {
                            StudentId = form.Student.Id,
                            ClassesId = cls.Id
                        };

                        await _studentRepo.AddStudentClassAsync(studentClass);
                    }

                        await _transactionRepo.CreateTransactionAsync(new Transaction
                    {
                        FormId = formId,
                        Amount = successTotal,
                        Status = "success",
                        Description = $"Registered classes successful",
                        TxnRef = vnp_TxnRef,
                        TransactionItems = successClasses.Select(c => new TransactionItem
                        {
                            Name = c.Name,
                            Cost = c.Cost ?? 0
                        }).ToList()
                    });
                }

                if (refundTotal > 0)
                {
                    await _transactionRepo.CreateTransactionAsync(new Transaction
                    {
                        FormId = formId,
                        Amount = refundTotal,
                        Status = "waiting_for_refund",
                        Description = $"Refund for full classes. Please contact the administrator to process your refund. Contact number: 0886122578",
                        TransactionItems = fullClasses.Select(c => new TransactionItem
                        {
                            Name = c.Name,
                            Cost = c.Cost ?? 0
                        }).ToList()
                    });

                    var fullClassNames = fullClasses.Select(c => c.Name).ToList();

                    var message = $"Enrollment in the following classes is no longer available: {string.Join(", ", fullClassNames)}. Please consult the transactions list for refund details.";

                    // Return the response
                    return new ResponseObject("conflict", message, null);

                }      
                return new ResponseObject("ok", "All classes registered successfully", null);
            }
            else
            {
                form.Status = "waiting_for_payment";

                await _admissionFormRepo.UpdateAdmissionFormAsync(form);
                
                return new ResponseObject("conflict", "Payment failed or canceled.", null);
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParentService.API.Utils;
using ParentService.Application.DTOs;
using ParentService.Application.DTOs.Request;
using ParentService.Application.Services;
using ParentService.Application.Services.IServices;
using ParentService.Domain.DTOs.Request;
using ParentService.Domain.IClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParentService.API.Controllers
{
    [Route("api/admissionForm")]
    [ApiController]
    public class AdmissionFormController : ControllerBase
    {
        private IAdmissionFormService _admissionFormService;
        private IVnPayService _vnPayService;
        public AdmissionFormController(IAdmissionFormService admissionFormService, IVnPayService vnPayService)
        {
            _admissionFormService = admissionFormService;
            _vnPayService = vnPayService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmissionFormAsync([FromBody] CreateFormRequest request)
        {
            var userId = Request.Headers["X-User-Id"].ToString();

            var result = await _admissionFormService.CreateAdmissionFormAsync(request, int.Parse(userId));

            return Ok(result);
        }
        [HttpPut("check/availability/classes")]
        public async Task<IActionResult> CheckClassesAvailabilityAsync([FromBody] CheckClassRequest request)
        {

            var result = await _admissionFormService.CheckClassesAvailabilityAsync(request);

            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }

            if (result.StatusResponseCode.Equals("conflict"))
            {
                return Conflict(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAdmissionForm([FromQuery] int afId)
        {
            var result = await _admissionFormService.DeleteAdmissionForm(afId);

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAdmissionFormsAsync()
        {
            var userId = Request.Headers["X-User-Id"].ToString();
            var result = await _admissionFormService.GetAdmissionFormsByParentAccountId(int.Parse(userId));
            return Ok(result);
        }

        [HttpPut("remove/class/list")]
        public async Task<IActionResult> RemoveClassesFromAdmissionForm(RemoveClassesFromAdmissionFormRequest request)
        {
            var result = await _admissionFormService.RemoveClassesFromAdmissionForm(request);

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }
            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("class/list/{afId}")]
        public async Task<IActionResult> GetClassesByAdmissionFormId(int afId)
        {
            var result = await _admissionFormService.GetClassesByAdmissionFormId(afId);
            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }
            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);

            }
            return Ok(result);
        }


        [HttpGet("paymentUrl/{id}")]
        public async Task<IActionResult> GetPaymentUrl(int id)
        {
          var result = await _vnPayService.GetPaymentUrl(IpHelper.GetIpAddress(HttpContext), id);

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }
            if (result.StatusResponseCode.Equals("conflict"))
            {
                return Conflict(result);
            }          
            return Ok(result);
        }

        [HttpGet("paymentUrl/confirm")]
        public async Task<IActionResult> ConfirmPaymentUrl([FromQuery] string vnp_Amount, string vnp_OrderInfo, string vnp_PayDate, string vnp_TransactionStatus, string vnp_TxnRef)
        {
            var result = await _vnPayService.ConfirmPaymentUrl(vnp_Amount, vnp_OrderInfo, vnp_PayDate, vnp_TransactionStatus, vnp_TxnRef);

            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }
            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }
            if (result.StatusResponseCode.Equals("conflict"))
            {
                return Conflict(result);
            }
            return Ok(result);
        }
    }
}

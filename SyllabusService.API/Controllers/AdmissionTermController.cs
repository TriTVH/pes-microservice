using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services;
using SyllabusService.Application.Services.IServices;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.API.Controllers
{
    [Route("api/term")]
    [ApiController]
    public class AdmissionTermController : ControllerBase
    {
        private IAdmissionTermService _admissionTermService;
        public AdmissionTermController(IAdmissionTermService admissionTermService)
        {
            _admissionTermService = admissionTermService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAdmissionTermAsync([FromBody] CreateAdmissionTermRequest request)
        {
            try
            {
                var result = await _admissionTermService.CreateAdmissionTermAsync(request);
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
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            }
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetAdmissionTermsAsync()
        {

            var result = await _admissionTermService.GetAllAdmissionTermsAsync();

            return Ok(result);

        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAdmissionTermAsync()
        {
            var result = await _admissionTermService.GetActiveAdmissionTermAsync();

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("common/{id}")]
        public async Task<IActionResult> GetAdmissionTermByIdAsync(int id)
        {
            var result = await _admissionTermService.GetAdmissionTermById(id);

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("comboBox")]
        public async Task<IActionResult> GetComboBoxAdmissionTermsAsync()
        {
            var result = await _admissionTermService.GetComboBoxAdmissionTermsAsync();
            return Ok(result);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateAdmissionTermStatus([FromBody] UpdateAdmissionTermActionRequest request)
        {
            var result = await _admissionTermService.UpdateAdmissionTermStatusByAction(request);

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

    }
}

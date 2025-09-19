using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TermAdmissionManagement.Application.DTOs;
using TermAdmissionManagement.Application.DTOs.Request;
using TermAdmissionManagement.Application.Services;
using TermAdmissionManagement.Application.Services.IService;

namespace TermAdmissionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermAdmissionController : ControllerBase
    {
        private readonly IAdmissionTermService _admissionTermService;

        public TermAdmissionController(IAdmissionTermService admissionTermService)
        {
            _admissionTermService = admissionTermService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdmissionTermRequest request)
        {
            try
            {
                var result = await _admissionTermService.CreateAdmissionTerm(request);
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
        [HttpPut]
        [Route("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] UpdateAdmissionTermStatus request)
        {
            return Ok();
        }

        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> GetAdmissionTerms()
        {
            try
            {
                return Ok(await _admissionTermService.GetAdmissionTerms());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            }
        }
    }
}

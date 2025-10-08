using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParentService.Application.DTOs.Request;
using ParentService.Application.Services;
using ParentService.Application.Services.IServices;

namespace ParentService.API.Controllers
{
    [Route("api/admissionForm")]
    [ApiController]
    public class AdmissionFormController : ControllerBase
    {
        private IAdmissionFormService _admissionFormService;
        public AdmissionFormController(IAdmissionFormService admissionFormService)
        {
            _admissionFormService = admissionFormService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmissionFormAsync([FromBody] CreateFormRequestWithNewStudentRequest request)
        {
            var userId = Request.Headers["X-User-Id"].ToString();

            var result = await _admissionFormService.CreateAdmissionFormAsync(request, int.Parse(userId));

            return Ok(result);
        }

    }
}

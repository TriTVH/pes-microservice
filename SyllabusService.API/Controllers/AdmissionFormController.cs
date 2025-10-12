using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services.IServices;

namespace SyllabusService.API.Controllers
{
    [Route("api/form")]
    [ApiController]
    public class AdmissionFormController : ControllerBase
    {
        private IAdmissionFormService _admissionFormService;
        public AdmissionFormController(IAdmissionFormService admissionFormService)
        {
            _admissionFormService = admissionFormService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAdmissionFormsAsync([FromQuery] int admissionTermId) 
        {
            return Ok(await _admissionFormService.GetAdmissionFormsByAdmissionTermIdAsync(admissionTermId));
        }

        [HttpPut]
        public async Task<IActionResult> ChangeStatusAdmissionFormByAction(UpdateAdmissionFormStatusRequest request)
        {
            var result = await _admissionFormService.ChangeStatusOfAdmissionForm(request);

            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

    }
}

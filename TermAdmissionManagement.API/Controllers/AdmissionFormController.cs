using Microsoft.AspNetCore.Mvc;
using TermAdmissionManagement.Application.DTOs;
using TermAdmissionManagement.Application.DTOs.Request;
using TermAdmissionManagement.Application.Services.IService;

namespace TermAdmissionManagement.API.Controllers
{
    [Route("api/form")]
    [ApiController]
    public class AdmissionFormController : ControllerBase
    {

        private readonly IAdmissionFormService _admissionFormService;

        public AdmissionFormController(IAdmissionFormService admissionFormService)
        {
             _admissionFormService = admissionFormService;
        }

        //[HttpGet]
        //[Route("list")]
        //public async Task<IActionResult> GetAdmissionForms()
        //{
        //    try
        //    {
        //        return Ok(await _admissionFormService.GetAdmissionFormsAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
        //    }
        //}

        [HttpPost]
        [Route("parent")]
        public async Task<IActionResult> CreateForm([FromBody] CreateAdmissionFormRequest request)
        {
            var userId = Request.Headers["X-User-Id"].ToString();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            int.TryParse(userId, out var id);
            try
            {
                ResponseObject result = await _admissionFormService.CreateAdmissionForm(request, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            }
        }
    }
}

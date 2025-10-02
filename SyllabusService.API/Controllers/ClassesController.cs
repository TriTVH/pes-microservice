using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services.IServices;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.API.Controllers
{
    [Route("api/classes")]
    [ApiController]
    public class ClassesController : ControllerBase
    {

        private IClassesServices _classesServices;

        public ClassesController(IClassesServices classesServices) 
        {
             _classesServices = classesServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            try
            {
                var result = await _classesServices.CreateClass(request);
            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            } else if (result.StatusResponseCode.Equals("errorConnection"))
            {
                return StatusCode(503, result);
            } else if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            } else if (result.StatusResponseCode.Equals("conflict")){
                return Conflict(result);
            }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            }
        }

        [HttpGet("list/after")]
        public async Task<IActionResult> GetClassesAfterDateInAcademicYear([FromQuery] DateOnly endDate)
        {
         
            var result = await _classesServices.GetClassesAfterDateInYearAsync(endDate);

            return  Ok(result);
        }
    }
}

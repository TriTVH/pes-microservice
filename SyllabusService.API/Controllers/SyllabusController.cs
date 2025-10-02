using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services.IServices;
using TermAdmissionManagement.Application.DTOs;

namespace SyllabusService.API.Controllers
{
    [Route("api/syllabus")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusService _syllabusService;

        public SyllabusController(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSyllabusRequest request)
        {
            try
            {
                var item = await _syllabusService.CreateSyllabusAsync(request);
          
                if (item.StatusResponseCode.ToLower().Equals("badRequest"))
                {
                    return BadRequest(item);
                }
                else if (item.StatusResponseCode.ToLower().Equals("conflict"))
                {
                    return Conflict(item);
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            } 
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSyllabusAsync()
        {
            return Ok(await _syllabusService.GetAllSyllabusAsync());
        }

        [HttpGet("list/active")]
        public async Task<IActionResult> GetAllActiveSyllabusAsync()
        {
            return Ok(await _syllabusService.GetAllActiveSyllabusAsync());
        }


        [HttpPut]
        public async Task<IActionResult> UpdateSyllabusAsync([FromBody] UpdateSyllabusRequest request)
        {
            try
            {
                var result = await _syllabusService.UpdateSyllabusAsync(request);

                if (result.StatusResponseCode.Equals("notFound"))
                {
                    return NotFound(result);
                }
                else if (result.StatusResponseCode.Equals("conflict"))
                {
                    return Conflict(result);
                }
                else if (result.StatusResponseCode.Equals("badRequest"))
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseObject(ex.Message, "Đã xảy ra lỗi khi xử lý yêu cầu.", null));
            }
        }
    }
}

using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParentService.Application.DTOs.Request;
using ParentService.Application.Services.IServices;

namespace ParentService.API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudentAsync([FromBody] CreateNewStudentRequest request)
        {

            try
            {

                var userId = Request.Headers["X-User-Id"].ToString();

                var result = await _studentService.CreateStudentAsync(request, int.Parse(userId));

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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {

            var userId = Request.Headers["X-User-Id"].ToString();

            var result = await _studentService.GetStudentsAsync(int.Parse(userId));

            return Ok(result);
        }

    }
}

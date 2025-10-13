using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParentService.Application.DTOs.Request;
using ParentService.Application.Services.IServices;
using ParentService.Domain.DTOs.Request;

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

        [HttpGet("list")]
        public async Task<IActionResult> GetAllStudents()
        {

            var userId = Request.Headers["X-User-Id"].ToString();

            var result = await _studentService.GetStudentsAsync(int.Parse(userId));

            return Ok(result);
        }

        [HttpGet("public/{id}")]
        public async Task<IActionResult> GetStudentByIdAsync(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            if (result.StatusResponseCode.Equals("notFound"))
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPut("activity/list")]
        public async Task<IActionResult> GetActivitiesBetweenStartDateAndEndDate(int studentId, WeekRequest request)
        {
            var result = await _studentService.GetActivitiesBetweenStartDateAndEndDate(studentId, request);
            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}

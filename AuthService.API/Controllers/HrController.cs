using Auth.Application.DTOs.Teacher;
using Auth.Infrastructure.Security;
using Auth.Services.Services.IServices;
using Auth.Services.Services;
using Auth.Services.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.API.Controllers
{
    [Route("api/Hr")]
    [ApiController]
    public class HrController : ControllerBase
    {
        private readonly IAuthService _svc;
        private readonly AuthServiceWrapper _authWrapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMemoryCache _cache;
        public HrController(
           IAuthService svc,
           AuthServiceWrapper authWrapper,
           IPasswordHasher passwordHasher,
           IMemoryCache cache)
        {
            _svc = svc;
            _authWrapper = authWrapper;
            _passwordHasher = passwordHasher;
            _cache = cache;
        }
        [Authorize(Roles = "HR")]
        [HttpGet("getAllAccount")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("getAllAccount/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var acc = await _svc.GetByIdAsync(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }
        [Authorize(Roles = "HR")]
        [HttpDelete("ban/{id}")]
        public async Task<IActionResult> BanAccount(int id)
        {
            await _svc.DeleteAsync(id); // will set Status = ACCOUNT_BAN
            return NoContent();
        }
        [Authorize(Roles = "HR")]
        [HttpDelete("unban/{id}")]
        public async Task<IActionResult> UnBanAccount(int id)
        {
            await _svc.UnBanAsync(id); // will set Status = ACCOUNT_UNBAN
            return NoContent();
        }
        [Authorize(Roles = "HR")]
        [HttpPost("teacher")]
        public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
        {
            var created = await _svc.CreateTeacherAsync(dto);
            return CreatedAtAction(nameof(GetTeacherById), new { id = created.Id }, created);
        }


        [Authorize(Roles = "HR")]
        [HttpPost("teacher/email_sending")]
        public async Task<IActionResult> CreateTeacherEmailOnly(CreateTeacherEmailOnlyDto dto)
        {
            var response = await _authWrapper.CreateTeacherEmailOnlyWithResponseAsync(dto);
            return response.ToApiResponse();
        }

        [Authorize(Roles = "HR")]
        [HttpPut("teacher/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto dto)
        {
            await _svc.UpdateTeacherAsync(id, dto);
            return NoContent();
        }

        [Authorize(Roles = "HR, EDUCATION")]
        [HttpGet("teacher")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var list = await _svc.GetTeachersAsync();
            return Ok(list);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("teacher/export")]
        public async Task<IActionResult> ExportTeachers()
        {
            var file = await _svc.ExportTeachersAsync();
            return File(file.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
        }

        // Parent list & export
        [Authorize(Roles = "HR")]
        [HttpGet("parent")]
        public async Task<IActionResult> GetParents()
        {
            var list = await _svc.GetParentsAsync();
            return Ok(list);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("parent/export")]
        public async Task<IActionResult> ExportParents()
        {
            var file = await _svc.ExportParentsAsync();
            return File(file.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
        }
        [Authorize(Roles = "HR, EDUCATION")]
        [HttpGet("teacher/{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _svc.GetByIdAsync(id);
            if (teacher == null || teacher.Role != "TEACHER")
                return NotFound();

            return Ok(teacher);
        }

    }
}

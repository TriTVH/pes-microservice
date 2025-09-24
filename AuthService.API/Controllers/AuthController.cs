using Auth.Application.DTOs;
using Auth.Application.DTOs.Teacher;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Security;
using Auth.Services.Services.IServices;
using AuthService.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _svc;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMemoryCache _cache;
    public AuthController(
       IAuthService svc,
       IPasswordHasher passwordHasher,
       IMemoryCache cache)
        {
            _svc = svc;
            _passwordHasher = passwordHasher;
            _cache = cache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto req)
        {
            var acc = await _svc.RegisterAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = acc.Id }, acc);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto req)
        {
            var token = await _svc.LoginAsync(req);
            return Ok(token);
        }

        //[Authorize(Roles = "HR")]
        //[HttpGet("getAllAccount")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var list = await _svc.GetAllAsync();
        //    return Ok(list);
        //}

        [Authorize(Roles = "HR")]
        [HttpGet("getAllAccount/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var acc = await _svc.GetByIdAsync(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }

        [Authorize]
        [HttpPut("updateProfile/{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountDto dto)
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }

        //[Authorize(Roles = "HR")]
        //[HttpDelete("ban/{id}")]
        //public async Task<IActionResult> BanAccount(int id)
        //{
        //    await _svc.DeleteAsync(id); // will set Status = ACCOUNT_BAN
        //    return NoContent();
        //}
        //[Authorize(Roles = "HR")]
        //[HttpDelete("unban/{id}")]
        //public async Task<IActionResult> UnBanAccount(int id)
        //{
        //    await _svc.UnBanAsync(id); // will set Status = ACCOUNT_UNBAN
        //    return NoContent();
        //}
        [Authorize(Roles = "PARENT,TEACHER")]
        [HttpPost("pass/forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _svc.ForgotPasswordAsync(request.Email);
            return Ok(new { message = "Password reset email sent successfully" });
        }
        [Authorize(Roles = "PARENT,TEACHER")]
        [HttpPost("pass/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] Auth.Application.DTOs.ResetPasswordRequest request)
        {
            await _svc.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> ViewProfile()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value
                ?? throw new Exception("Missing id claim"));

            var profile = await _svc.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value
                ?? throw new Exception("Missing id claim"));

            await _svc.UpdateProfileAsync(userId, dto);
            return NoContent();
        }
        //[Authorize(Roles = "HR")]
        //[HttpGet("teacher/{id}")]
        //public async Task<IActionResult> GetTeacherById(int id)
        //{
        //    var teacher = await _svc.GetByIdAsync(id);
        //    if (teacher == null || teacher.Role != "TEACHER")
        //        return NotFound();

        //    return Ok(teacher);
        //}
        
        //[Authorize(Roles = "HR")]
        //[HttpPost("teacher")]
        //public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
        //{
        //    var created = await _svc.CreateTeacherAsync(dto);
        //    return CreatedAtAction(nameof(GetTeacherById), new { id = created.Id }, created);
        //}

        //[Authorize(Roles = "HR")]
        //[HttpPut("teacher/{id}")]
        //public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto dto)
        //{
        //    await _svc.UpdateTeacherAsync(id, dto);
        //    return NoContent();
        //}

        //[Authorize(Roles = "HR")] 
        //[HttpGet("teacher")]
        //public async Task<IActionResult> GetAllTeachers()
        //{
        //    var list = await _svc.GetTeachersAsync();
        //    return Ok(list);
        //}

        //[Authorize(Roles = "HR")]
        //[HttpGet("teacher/export")]
        //public async Task<IActionResult> ExportTeachers()
        //{
        //    var file = await _svc.ExportTeachersAsync();
        //    return File(file.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
        //}

        //// Parent list & export
        //[Authorize(Roles = "HR")]
        //[HttpGet("parent")]
        //public async Task<IActionResult> GetParents()
        //{
        //    var list = await _svc.GetParentsAsync();
        //    return Ok(list);
        //}

        //[Authorize(Roles = "HR")]
        //[HttpGet("parent/export")]
        //public async Task<IActionResult> ExportParents()
        //{
        //    var file = await _svc.ExportParentsAsync();
        //    return File(file.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
        //}

    }
}


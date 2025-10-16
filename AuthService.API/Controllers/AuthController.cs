using Auth.Application.DTOs;
using Auth.Application.DTOs.Teacher;
using Auth.Application.DTOs.Parent;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Security;
using Auth.Services.Services.IServices;
using Auth.Services.Services;
using Auth.Services.Extensions;
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
        private readonly AuthServiceWrapper _authWrapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMemoryCache _cache;
    public AuthController(
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto req)
        {
            var response = await _authWrapper.RegisterWithResponseAsync(req);
            return response.ToApiResponse();
        }

        [HttpPost("register-parent")]
        public async Task<IActionResult> RegisterParent(RegisterParentRequestDto req)
        {
            var response = await _authWrapper.RegisterParentWithResponseAsync(req);
            return response.ToApiResponse();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto req)
        {
            var response = await _authWrapper.LoginWithResponseAsync(req);
            return response.ToApiResponse();
        }

     

        [Authorize(Roles = "HR, EDUCATION")]
        [HttpGet("getAllAccount/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _authWrapper.GetAccountByIdWithResponseAsync(id);
            return response.ToApiResponse();
        }

        [Authorize]
        [HttpPut("updateProfile/{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountDto dto)
        {
            var response = await _authWrapper.UpdateAccountWithResponseAsync(id, dto);
            return response.ToApiResponse();
        }

       

        [HttpPost("pass/forgot")]
        public async Task<IActionResult> ForgotPasswordSimple([FromBody] ForgotPasswordSimpleRequestDto request)
        {
            var response = await _authWrapper.ForgotPasswordSimpleWithResponseAsync(request);
            return response.ToApiResponse();
        }
        [HttpPost("pass/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] Auth.Application.DTOs.ResetPasswordRequest request)
        {
            await _svc.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value
                ?? throw new Exception("Missing id claim"));

            await _svc.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Password changed successfully" });
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

        [HttpPost("first-login-completed")]
        [Authorize]
        public async Task<IActionResult> MarkFirstLoginCompleted()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value
                ?? throw new Exception("Missing id claim"));

            await _svc.MarkFirstLoginCompletedAsync(userId);
            return Ok(new { message = "First login marked as completed" });
        }
       

    }
}


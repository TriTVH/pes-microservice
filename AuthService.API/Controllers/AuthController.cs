using Auth.Application.DTOs;
using Auth.Services.Services.IServices;
using AuthService.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _svc;
        public AuthController(IAuthService svc) => _svc = svc;

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

        [Authorize(Roles = "HR")]
        [HttpGet("getAllAccount")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var acc = await _svc.GetByIdAsync(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }

        [Authorize(Roles = "EDUCATION")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountDto dto)
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }

        [Authorize(Roles = "HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BanAccount(int id)
        {
            await _svc.DeleteAsync(id); // will set Status = ACCOUNT_BAN
            return NoContent();
        }
        [Authorize(Roles = "HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> UnBanAccount(int id)
        {
            await _svc.UnBanAsync(id); // will set Status = ACCOUNT_UNBAN
            return NoContent();
        }
    }
}


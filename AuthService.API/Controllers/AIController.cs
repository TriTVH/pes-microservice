using Auth.Services.DTOs.AI;
using Auth.Services.Services;
using Auth.Services.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly AIServiceWrapper _aiWrapper;

        public AIController(AIServiceWrapper aiWrapper)
        {
            _aiWrapper = aiWrapper;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            try
            {
                // Get user role from JWT token
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                request.UserRole = userRole;

                var response = await _aiWrapper.ChatWithResponseAsync(request);
                return response.ToApiResponse();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"Chat request failed: {ex.Message}",
                    data = (object?)null,
                    errorCode = "CHAT_REQUEST_ERROR",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("system-prompt")]
        [Authorize(Roles = "HR, EDUCATION")]
        public async Task<IActionResult> GetSystemPrompt()
        {
            var response = await _aiWrapper.GetSystemPromptWithResponseAsync();
            return response.ToApiResponse();
        }

        [HttpPost("chat/guest")]
        [AllowAnonymous]
        public async Task<IActionResult> ChatGuest([FromBody] ChatRequestDto request)
        {
            try
            {
                // Guest users have limited access
                request.UserRole = "GUEST";

                var response = await _aiWrapper.ChatWithResponseAsync(request);
                return response.ToApiResponse();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"Guest chat request failed: {ex.Message}",
                    data = (object?)null,
                    errorCode = "GUEST_CHAT_ERROR",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("test-database")]
        [AllowAnonymous]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var result = await _aiWrapper.TestDatabaseConnectionAsync();
                return Ok(new
                {
                    isSuccess = true,
                    message = "Database test completed",
                    data = result,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"Database test failed: {ex.Message}",
                    data = (object?)null,
                    errorCode = "DATABASE_TEST_ERROR",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}

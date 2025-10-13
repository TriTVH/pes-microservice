using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services;
using SyllabusService.Application.Services.IServices;

namespace SyllabusService.API.Controllers
{
    [Route("api/activity")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetActivitiesOfSchedule([FromQuery] int scheduleId)
        {
            return Ok(await _activityService.GetAllActivitiesByScheduleId(scheduleId));
        }

        [HttpPut("common/list")]
        public async Task<IActionResult> GetActivitiesBetweenStartDateAndEndDate([FromBody] GetActivitiesBetweenStartDateAndEndDateRequest request)
        {
            var result = await _activityService.GetActivitiesBetweenStartDateAndEndDate(request);
            if (result.StatusResponseCode.Equals("badRequest"))
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyllabusService.Application.Services;
using SyllabusService.Application.Services.IServices;

namespace SyllabusService.API.Controllers
{
    [Route("api/week")]
    [ApiController]
    public class WeekController : ControllerBase
    {
        private IWeekService _weekService;
        public WeekController(IWeekService weekService) 
        {
            _weekService = weekService;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetWeeksOfClass([FromQuery] int classId)
        {
            return Ok(await _weekService.GetScheduleByClassId(classId));
        }
    }
}

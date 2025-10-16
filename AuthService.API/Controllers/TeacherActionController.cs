using Auth.Application.Services.IServices;
using Auth.Services.Services;
using Auth.Services.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "TEACHER")]
public class TeacherActionController : ControllerBase
{
    private readonly ITeacherActionService _teacherActionService;
    private readonly TeacherActionServiceWrapper _teacherActionWrapper;

    public TeacherActionController(ITeacherActionService teacherActionService, TeacherActionServiceWrapper teacherActionWrapper)
    {
        _teacherActionService = teacherActionService;
        _teacherActionWrapper = teacherActionWrapper;
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var response = await _teacherActionWrapper.GetClassesWithResponseAsync(teacherId);
        return response.ToApiResponse();
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var response = await _teacherActionWrapper.GetSchedulesWithResponseAsync(teacherId);
        return response.ToApiResponse();
    }
    [HttpGet("weekly-schedule")]
    public async Task<IActionResult> GetWeeklySchedule([FromQuery] string weekName = "Week - 1")
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var response = await _teacherActionWrapper.GetWeeklyScheduleWithResponseAsync(teacherId, weekName);
        return response.ToApiResponse();
    }
    [HttpGet("classes/{classId}")]
    public async Task<IActionResult> GetClassDetail(int classId)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var response = await _teacherActionWrapper.GetClassDetailWithResponseAsync(classId, teacherId);
        return response.ToApiResponse();
    }
}

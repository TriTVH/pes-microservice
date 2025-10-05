using Auth.Application.Services.IServices;
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

    public TeacherActionController(ITeacherActionService teacherActionService)
    {
        _teacherActionService = teacherActionService;
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var classes = await _teacherActionService.GetClassesAsync(teacherId);
        return Ok(classes);
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var schedules = await _teacherActionService.GetSchedulesAsync(teacherId);
        return Ok(schedules);
    }

    //[HttpGet("classes/{classId}")]
    //public async Task<IActionResult> GetClassDetail(int classId)
    //{
    //    var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    //    var classDetail = await _teacherActionService.GetClassDetailAsync(classId, teacherId);
    //    if (classDetail == null) return NotFound();
    //    return Ok(classDetail);
    //}
}

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SyllabusService.API.Controllers;
using SyllabusService.Application.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace Unit.Tests.Controllers
{
    public class WeekControllerTest
    {
        private readonly Mock<IWeekService> _mockService;
        private readonly WeekController _controller;

        public WeekControllerTest()
        {
            _mockService = new Mock<IWeekService>();
            _controller = new WeekController(_mockService.Object);
        }

        [Fact]
        public async Task GetWeeksOfClass_ShouldReturnOk_WithSchedules()
        {
            // Arrange
            var classId = 10;

            var schedules = new List<object>
    {
        new { DayOfWeek = "MONDAY", StartTime = "09:00", EndTime = "11:00" },
        new { DayOfWeek = "WEDNESDAY", StartTime = "14:00", EndTime = "16:00" }
    };

            var fakeResponse = new ResponseObject(
                "ok",
                "Get schedules successfully",
                schedules
            );

            _mockService.Setup(s => s.GetScheduleByClassId(classId))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetWeeksOfClass(classId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult!.StatusCode.Should().Be(200);
            var response = okResult.Value as ResponseObject;
            response!.StatusResponseCode.Should().Be("ok");
            response.Message.Should().Contain("successfully");
            response.Data.Should().BeEquivalentTo(schedules);

            _mockService.Verify(s => s.GetScheduleByClassId(classId), Times.Once);
        }
        [Fact]
        public async Task GetWeeksOfClass_ShouldReturnOk_WithEmptyList_WhenNoSchedulesFound()
        {
            // Arrange
            var classId = 999;
            var emptyList = new List<object>();

            var fakeResponse = new ResponseObject(
                "ok",
                "No schedules found",
                emptyList
            );

            _mockService.Setup(s => s.GetScheduleByClassId(classId))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetWeeksOfClass(classId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as ResponseObject;
            response.Should().NotBeNull();
            response!.StatusResponseCode.Should().Be("ok");
            response.Message.Should().Contain("No schedules");
          

            _mockService.Verify(s => s.GetScheduleByClassId(classId), Times.Once);
        }
    }
}

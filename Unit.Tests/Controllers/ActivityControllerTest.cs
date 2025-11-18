using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SyllabusService.API.Controllers;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;
using Xunit;

namespace Unit.Tests.Controllers
{
    public class ActivityControllerTest
    {
        private readonly ActivityController _controller;
        private readonly Mock<IActivityService> _mockService;

        public ActivityControllerTest()
        {
            _mockService = new Mock<IActivityService>();
            _controller = new ActivityController(_mockService.Object);
        }

        // ✅ GET: /api/activity/list
        [Fact]
        public async Task GetActivitiesOfSchedule_ShouldReturnOk_WithActivities()
        {
            // Arrange
            int scheduleId = 5;
            var fakeResponse = new ResponseObject("ok", "Activities retrieved successfully", new List<ActivityDTO>
            {
                new ActivityDTO { Id = 1, Name = "Drawing" },
                new ActivityDTO { Id = 2, Name = "Singing" }
            });

            _mockService.Setup(s => s.GetAllActivitiesByScheduleId(scheduleId))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetActivitiesOfSchedule(scheduleId);

            // Assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var data = ok.Value as ResponseObject;
            data!.StatusResponseCode.Should().Be("ok");
            ((List<ActivityDTO>)data.Data!).Should().HaveCount(2);
        }

        // ✅ PUT: /api/activity/common/list - Success
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?> { 1, 2 },
                startWeek = new DateOnly(2025, 10, 1),
                endWeek = new DateOnly(2025, 10, 7)
            };

            var fakeResponse = new ResponseObject("ok", "Fetched successfully", new List<ActivityDTO>
            {
                new ActivityDTO { Id = 1, Name = "Play time" }
            });

            _mockService.Setup(s => s.GetActivitiesBetweenStartDateAndEndDate(request))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetActivitiesBetweenStartDateAndEndDate(request);

            // Assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var data = ok.Value as ResponseObject;
            data!.Message.Should().Be("Fetched successfully");
        }

        // ✅ PUT: /api/activity/common/list - BadRequest
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnBadRequest_WhenInvalidDateRange()
        {
            // Arrange
            var request = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?>(),
                startWeek = new DateOnly(2025, 10, 10),
                endWeek = new DateOnly(2025, 10, 5) // invalid range
            };

            var fakeResponse = new ResponseObject("badRequest", "Invalid date range", null);

            _mockService.Setup(s => s.GetActivitiesBetweenStartDateAndEndDate(request))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetActivitiesBetweenStartDateAndEndDate(request);

            // Assert
            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.StatusCode.Should().Be(400);
        }

        // ✅ PUT: /api/activity/common/list - Exception
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            var request = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?> { 1 },
                startWeek = new DateOnly(2025, 10, 10),
                endWeek = new DateOnly(2025, 10, 15)
            };

            _mockService.Setup(s => s.GetActivitiesBetweenStartDateAndEndDate(request))
                        .ThrowsAsync(new Exception("DB Error"));

            // Act
            Func<Task> act = async () => await _controller.GetActivitiesBetweenStartDateAndEndDate(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
        }
    }

  
}

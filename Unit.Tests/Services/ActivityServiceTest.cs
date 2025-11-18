using FluentAssertions;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.Services
{
    public class ActivityServiceTest
    {
        private readonly Mock<IActivityRepository> _mockRepo;
        private readonly ActivityService _service;

        public ActivityServiceTest()
        {
            _mockRepo = new Mock<IActivityRepository>();
            _service = new ActivityService(_mockRepo.Object);
        }

        // ✅ 1️⃣ Test GetAllActivitiesByScheduleId (normal case)
        [Fact]
        public async Task GetAllActivitiesByScheduleId_ShouldReturnOk_WithMappedActivities()
        {
            // Arrange
            var scheduleId = 100;
            var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 1,
                    Name = "Lecture 1",
                    Date = new DateOnly(2025, 1, 1),
                    DayOfWeek = "Monday",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0)
                },
                new Activity
                {
                    Id = 2,
                    Name = "Lecture 2",
                    Date = new DateOnly(2025, 1, 2),
                    DayOfWeek = "Tuesday",
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(11, 0)
                }
            };

            _mockRepo.Setup(r => r.GetActivitiesByScheduleIdAsync(scheduleId))
                .ReturnsAsync(activities);

            // Act
            var result = await _service.GetAllActivitiesByScheduleId(scheduleId);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("View all activities of schedule successfully");
            result.Data.Should().BeAssignableTo<IEnumerable<ActivityDTO>>();

            var data = (IEnumerable<ActivityDTO>)result.Data!;
            data.Should().HaveCount(2);
            data.First().DayOfWeek.Should().Be("Monday");
        }

        // ✅ 2️⃣ Test GetActivitiesBetweenStartDateAndEndDate — empty classIds
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnBadRequest_WhenClassIdsEmpty()
        {
            // Arrange
            var req = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?>(), // empty list
                startWeek = DateOnly.FromDateTime(DateTime.UtcNow),
                endWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            };

            // Act
            var result = await _service.GetActivitiesBetweenStartDateAndEndDate(req);

            // Assert
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Class IDs cannot be empty or null.");
        }

        // ✅ 3️⃣ Test GetActivitiesBetweenStartDateAndEndDate — startWeek > endWeek
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnBadRequest_WhenStartAfterEnd()
        {
            // Arrange
            var req = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?> { 1, 2 },
                startWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), // start > end
                endWeek = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Act
            var result = await _service.GetActivitiesBetweenStartDateAndEndDate(req);

            // Assert
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Start week cannot be later than end week.");
        }

        // ✅ 4️⃣ Test GetActivitiesBetweenStartDateAndEndDate — valid request
        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnOk_WithMappedDTOs()
        {
            // Arrange
            var req = new GetActivitiesBetweenStartDateAndEndDateRequest
            {
                classIds = new List<int?> { 1, 2 },
                startWeek = DateOnly.FromDateTime(DateTime.UtcNow),
                endWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            };

            var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 1,
                    Name = "Lab 1",
                    Date = new DateOnly(2025, 2, 1),
                    DayOfWeek = "Friday",
                    StartTime = new TimeOnly(13, 0),
                    EndTime = new TimeOnly(14, 0)
                }
            };

            _mockRepo.Setup(r => r.GetActivitiesBetweenStartDateAndEndDate(
                req.classIds, req.startWeek, req.endWeek))
                .ReturnsAsync(activities);

            // Act
            var result = await _service.GetActivitiesBetweenStartDateAndEndDate(req);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get Activities Between Start Week And End Week Successfully");
            result.Data.Should().BeAssignableTo<IEnumerable<ActivityDTO>>();

            var data = (IEnumerable<ActivityDTO>)result.Data!;
            data.Should().ContainSingle();
            data.First().Name.Should().Be("Lab 1");
        }
    }
}

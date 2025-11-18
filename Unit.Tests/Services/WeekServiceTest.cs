using FluentAssertions;
using Moq;
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
    public class WeekServiceTest
    {
        private readonly Mock<IWeekRepository> _mockRepo;
        private readonly WeekService _service;

        public WeekServiceTest()
        {
            _mockRepo = new Mock<IWeekRepository>();
            _service = new WeekService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetScheduleByClassId_ShouldReturnOk_WithMappedWeekDTOs()
        {
            // Arrange
            var schedules = new List<Schedule>
            {
                new Schedule
                {
                    Id = 1,
                    WeekName = "Week 1",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 1, 7)
                },
                new Schedule
                {
                    Id = 2,
                    WeekName = "Week 2",
                    StartDate = new DateTime(2025, 1, 8),
                    EndDate = new DateTime(2025, 1, 14)
                }
            };

            _mockRepo.Setup(r => r.GetSchedulesByClassIdAsync(100))
                .ReturnsAsync(schedules);

            // Act
            var result = await _service.GetScheduleByClassId(100);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get schedules of class successfully");
            result.Data.Should().BeAssignableTo<IEnumerable<Schedule>>();

            var data = (IEnumerable<Schedule>)result.Data;
            data.Should().HaveCount(2);
            data.Should().Contain(s => s.WeekName == "Week 1");
        }

        [Fact]
        public async Task GetScheduleByClassId_ShouldReturnOk_WhenNoSchedulesFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetSchedulesByClassIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Schedule>());

            // Act
            var result = await _service.GetScheduleByClassId(999);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get schedules of class successfully");
            ((IEnumerable<Schedule>)result.Data).Should().BeEmpty();
        }
    }
}

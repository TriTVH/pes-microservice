using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Tests.Controllers
{
    public class ClassControllerTest
    {
        private readonly Mock<IClassRepository> _mockClassRepo;
        private readonly Mock<ISyllabusRepository> _mockSyllabusRepo;
        private readonly Mock<IAuthClient> _mockAuthClient;
        private readonly ClassesService _service;

        public ClassControllerTest()
        {
            _mockClassRepo = new Mock<IClassRepository>();
            _mockSyllabusRepo = new Mock<ISyllabusRepository>();
            _mockAuthClient = new Mock<IAuthClient>();

            _service = new ClassesService(
                _mockSyllabusRepo.Object,
                _mockClassRepo.Object,
                _mockAuthClient.Object
            );
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_StartDateInvalid()
        {
           
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), 1, 2, new List<ActivityRequest>
            {
                new("Monday", new TimeOnly(8,0), new TimeOnly(09,0))
            });
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Start date cannot be in the past.");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidNumberActivitiesSmallThanZero()
        {
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 1, 2, new List<ActivityRequest>());
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Please add at least one activity");
        }
        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidRange()
        {
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 1, 2, new List<ActivityRequest>
    {
        new("Monday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tuesday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0)),
        new("Friday", new TimeOnly(14, 0), new TimeOnly(15, 0)) 
    }
);
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("You can add up to 4 activities");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidNameDayOfWeek()
        {
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 1, 2, new List<ActivityRequest>
    {
        new("Monday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tusday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
);
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("You can add up to 4 activities");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSortingDayOfWeek()
        {
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 1, 2, new List<ActivityRequest>
    {
        new("Monday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tusday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
);
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("You can add up to 4 activities");
        }
    }
}

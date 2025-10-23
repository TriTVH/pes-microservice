using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services;
using SyllabusService.Domain.DTOs;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;
using Xunit;

namespace SyllabusService.Tests.Controllers
{
    public class SyllabusControllerTest 
    {
        private readonly Mock<ISyllabusRepository> _mockRepo;
        private readonly SyllabusServ _service;

        public SyllabusControllerTest()
        {
            _mockRepo = new Mock<ISyllabusRepository>();
            _service = new SyllabusServ(_mockRepo.Object);
        }

        [Fact]
        public async Task POST_ShouldReturnStatusConflict()
        {
            _mockRepo.Setup(r => r.GetSyllabusByNameAsync("PRN222"))
                .ReturnsAsync(new Syllabus { Id = 1, Name = "PRN222" });

            var req = new CreateSyllabusRequest("PRN222", "EF Core", 200000, 20);
            var result = await _service.CreateSyllabusAsync(req);

            result.StatusResponseCode.Should().Be("conflict");
        }

        [Fact]
        public async Task POST_ShouldReturnStatusOk()
        {
            _mockRepo.Setup(r => r.GetSyllabusByNameAsync("PRN222"))
                   .ReturnsAsync((Syllabus?)null);

            var req = new CreateSyllabusRequest("PRN222", "EF Core", 200000, 20);
            var result = await _service.CreateSyllabusAsync(req);

            result.StatusResponseCode.Should().Be("ok");
        }

        [Fact]
        public async Task CreateSyllabus_ShouldReturnBadRequest_WhenInvalidData_CostSmallerThanDefault()
        {
            var request = new CreateSyllabusRequest("PRN212", "Hello", 50000, 5); // cost < 100000

            var result = await _service.CreateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Cost of syllabus must be greater than 100,000 VND");
        }

        [Fact]
        public async Task CreateSyllabus_ShouldReturnBadRequest_WhenInvalidData_EmptyName()
        {
            var request = new CreateSyllabusRequest("", "Hello", 120000, 5); // cost < 100000

            var result = await _service.CreateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Syllabus name must not be empty.");
        }

        [Fact]
        public async Task CreateSyllabus_ShouldReturnBadRequest_WhenInvalidData_EmptyDescription()
        {
            var request = new CreateSyllabusRequest("PRN212", "", 120000, 5); // cost < 100000

            var result = await _service.CreateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Syllabus description must not be empty.");
        }
        [Fact]
        public async Task CreateSyllabus_ShouldReturnBadRequest_WhenInvalidHoursOfSyllabus()
        {
            var request = new CreateSyllabusRequest("PRN212", "Hello", 120000, 5); // cost < 100000

            var result = await _service.CreateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Hours of syllabus must be greater than 10 and not greater than 40.");
        }




        [Fact]
        public async Task GetAllSyllabus_ShouldReturnList_WhenExists()
        {
            _mockRepo.Setup(r => r.GetAllSyllabusAsync())
                     .ReturnsAsync(new List<Syllabus>
                     {
                         new() { Id = 1, Name = "PRN222", Description = "EF", Cost = 200000, HoursOfSyllabus = 20, IsActive = true },
                         new() { Id = 2, Name = "PRN231", Description = "Web API", Cost = 250000, HoursOfSyllabus = 30, IsActive = false }
                     });

            var result = await _service.GetAllSyllabusAsync();

            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("successfully");
            result.Data.Should().NotBeNull();

            var syllabi = (IEnumerable<object>)result.Data!;
            syllabi.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateSyllabus_ShouldReturnBadRequest_WhenInvalid()
        {
            var request = new UpdateSyllabusRequest(1, "", "", 500000, 5, "");

            var result = await _service.UpdateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Syllabus name must not be empty");
        }

        [Fact]
        public async Task UpdateSyllabus_ShouldReturnNotFound_WhenNotExist()
        {
            var request = new UpdateSyllabusRequest(1, "PRN222", "EF Core", 500000, 20, "true");

            _mockRepo.Setup(r => r.GetSyllabusByIdAsync(1))
                     .ReturnsAsync((Syllabus?)null);

            var result = await _service.UpdateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("notFound");
        }

        [Fact]
        public async Task UpdateSyllabus_ShouldReturnConflict_WhenDuplicateName()
        {
            var request = new UpdateSyllabusRequest(1, "PRN222", "EF Core", 200000, 20, "true");


            _mockRepo.Setup(r => r.GetSyllabusByIdAsync(1))
                     .ReturnsAsync(new Syllabus { Id = 1, Name = "Old" });

            _mockRepo.Setup(r => r.IsDuplicateNameAsync("PRN222", 1))
                     .ReturnsAsync(true);

            var result = await _service.UpdateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("conflict");

        }

        [Fact]
        public async Task UpdateSyllabus_ShouldReturnOk_WhenValid()
        {
            var request = new UpdateSyllabusRequest(1, "PRN231", "Web API", 250000, 25, "true");
        
            var syllabus = new Syllabus
            {
                Id = 1,
                Name = "OldName",
                Description = "Old",
                Cost = 150000,
                HoursOfSyllabus = 15,
                IsActive = false
            };

            _mockRepo.Setup(r => r.GetSyllabusByIdAsync(1)).ReturnsAsync(syllabus);
            _mockRepo.Setup(r => r.IsDuplicateNameAsync("PRN231", 1)).ReturnsAsync(false);

            var result = await _service.UpdateSyllabusAsync(request);

            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Update Syllabus Successfully");

            _mockRepo.Verify(r => r.UpdateSyllabusAsync(It.Is<Syllabus>(s => s.Name == "PRN231")), Times.Once);
        }

    }
}

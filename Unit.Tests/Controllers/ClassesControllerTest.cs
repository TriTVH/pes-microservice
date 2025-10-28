using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SyllabusService.API.Controllers;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace Unit.Tests.Controllers
{
    public class ClassesControllerTest
    {
        private readonly Mock<IClassesServices> _mockService;
        private readonly ClassesController _controller;

        public ClassesControllerTest()
        {
            _mockService = new Mock<IClassesServices>();
            _controller = new ClassesController(_mockService.Object);
        }

        // ✅ POST CreateClass - notFound
        [Fact]
        public async Task CreateClass_ShouldReturnNotFound_WhenNotFound()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            var fakeResponse = new ResponseObject("notFound", "Class not found", null);
            _mockService.Setup(s => s.CreateClass(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<NotFoundObjectResult>();
            var res = result as NotFoundObjectResult;
            res!.StatusCode.Should().Be(404);
        }

        // ✅ POST CreateClass - errorConnection
        [Fact]
        public async Task CreateClass_ShouldReturn503_WhenConnectionError()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            var fakeResponse = new ResponseObject("errorConnection", "Service unavailable", null);
            _mockService.Setup(s => s.CreateClass(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(503);
        }

        // ✅ POST CreateClass - badRequest
        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_WhenInvalidRequest()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            var fakeResponse = new ResponseObject("badRequest", "Invalid data", null);
            _mockService.Setup(s => s.CreateClass(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.StatusCode.Should().Be(400);
        }

        // ✅ POST CreateClass - conflict
        [Fact]
        public async Task CreateClass_ShouldReturnConflict_WhenConflictOccurs()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            var fakeResponse = new ResponseObject("conflict", "Duplicated class", null);
            _mockService.Setup(s => s.CreateClass(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<ConflictObjectResult>();
            (result as ConflictObjectResult)!.StatusCode.Should().Be(409);
        }

        // ✅ POST CreateClass - success
        [Fact]
        public async Task CreateClass_ShouldReturnOk_WhenSuccessful()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            var fakeResponse = new ResponseObject("ok", "Created successfully", new { Id = 1 });
            _mockService.Setup(s => s.CreateClass(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.StatusCode.Should().Be(200);
        }

        // ✅ POST CreateClass - exception handling
        [Fact]
        public async Task CreateClass_ShouldReturn500_WhenExceptionThrown()
        {
            var request = new CreateClassRequest(DateOnly.FromDateTime(DateTime.Now), 1, 1, new List<ActivityRequest>());
            _mockService.Setup(s => s.CreateClass(request)).ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.CreateClass(request);

            result.Should().BeOfType<ObjectResult>();
            var res = result as ObjectResult;
            res!.StatusCode.Should().Be(500);
            (res.Value as ResponseObject)!.StatusResponseCode.Should().Contain("DB Error");
        }

        // ✅ GET list/after
        [Fact]
        public async Task GetClassesAfterDateInAcademicYear_ShouldReturnOk()
        {
            var date = DateOnly.FromDateTime(DateTime.Now);
            var fakeResponse = new ResponseObject("ok", "List classes after date", new List<object>());
            _mockService.Setup(s => s.GetClassesAfterDateInYearAsync(date))
                        .ReturnsAsync(fakeResponse);

            var result = await _controller.GetClassesAfterDateInAcademicYear(date);

            result.Should().BeOfType<OkObjectResult>();
        }

        // ✅ GET list
        [Fact]
        public async Task GetClasses_ShouldReturnOk()
        {
            var fakeResponse = new ResponseObject("ok", "All classes", new List<object>());
            _mockService.Setup(s => s.GetAllClassesAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetClasses();

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.StatusCode.Should().Be(200);
        }

        // ✅ GET public/{id} - not found
        [Fact]
        public async Task GetClassById_ShouldReturnNotFound_WhenNotFound()
        {
            var fakeResponse = new ResponseObject("notFound", "Class not found", null);
            _mockService.Setup(s => s.GetClassByIdAsync(5)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetClassById(5);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ✅ GET public/{id} - ok
        [Fact]
        public async Task GetClassById_ShouldReturnOk_WhenFound()
        {
            var fakeResponse = new ResponseObject("ok", "Found class", new { Id = 1 });
            _mockService.Setup(s => s.GetClassByIdAsync(1)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetClassById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // ✅ PUT public/by-ids - badRequest
        [Fact]
        public async Task GetClassesByIds_ShouldReturnBadRequest_WhenIdsInvalid()
        {
            var ids = new List<int>();
            var fakeResponse = new ResponseObject("badRequest", "Ids cannot be empty", null);
            _mockService.Setup(s => s.GetClassesByIds(ids)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetClassesByIds(ids);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ✅ PUT public/by-ids - ok
        [Fact]
        public async Task GetClassesByIds_ShouldReturnOk_WhenValid()
        {
            var ids = new List<int> { 1, 2 };
            var fakeResponse = new ResponseObject("ok", "Got classes successfully", new List<object>());
            _mockService.Setup(s => s.GetClassesByIds(ids)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetClassesByIds(ids);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}

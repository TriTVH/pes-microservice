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
    public class SyllabusControllerTest
    {
        private readonly SyllabusController _controller;
        private readonly Mock<ISyllabusService> _mockService;

        public SyllabusControllerTest()
        {
            _mockService = new Mock<ISyllabusService>();
            _controller = new SyllabusController(_mockService.Object);
        }

        // ✅ POST: Create - Success
        [Fact]
        public async Task Create_ShouldReturnOk_WhenValidData()
        {
            var req = new CreateSyllabusRequest("OOP", "Object Oriented", 200000, 30);
            var response = new ResponseObject("ok", "Created successfully", req);

            _mockService.Setup(s => s.CreateSyllabusAsync(req)).ReturnsAsync(response);

            var result = await _controller.Create(req);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var data = ok.Value as ResponseObject;
            data!.StatusResponseCode.Should().Be("ok");
            data.Message.Should().Be("Created successfully");
        }

        // ✅ POST: Create - BadRequest
        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenInvalidData()
        {
            var req = new CreateSyllabusRequest("", "", 0, 0);
            var fakeResponse = new ResponseObject("badRequest", "Invalid data", null);

            _mockService.Setup(s => s.CreateSyllabusAsync(It.IsAny<CreateSyllabusRequest>()))
                        .ReturnsAsync(fakeResponse);

            var result = await _controller.Create(req);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.StatusCode.Should().Be(400);
        }

        // ✅ POST: Create - Conflict
        [Fact]
        public async Task Create_ShouldReturnConflict_WhenDuplicate()
        {
            var req = new CreateSyllabusRequest("OOP", "Duplicate", 100000, 20);
            var fakeResponse = new ResponseObject("conflict", "Syllabus name already exists", null);

            _mockService.Setup(s => s.CreateSyllabusAsync(req))
                        .ReturnsAsync(fakeResponse);

            var result = await _controller.Create(req);

            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.StatusCode.Should().Be(409);
        }

        // ✅ POST: Create - Exception
        [Fact]
        public async Task Create_ShouldReturnServerError_OnException()
        {
            var req = new CreateSyllabusRequest("OOP", "Desc", 100, 10);
            _mockService.Setup(s => s.CreateSyllabusAsync(req))
                        .ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.Create(req);

            var error = result as ObjectResult;
            error.Should().NotBeNull();
            error!.StatusCode.Should().Be(500);
        }

        // ✅ GET: GetAllSyllabus
        [Fact]
        public async Task GetAllSyllabusAsync_ShouldReturnOk_WithData()
        {
            var fakeResponse = new ResponseObject("ok", "All syllabuses", new List<SyllabusDTO>
            {
                new SyllabusDTO { Id = 1, Name = "OOP" },
                new SyllabusDTO { Id = 2, Name = "PRN221" }
            });

            _mockService.Setup(s => s.GetAllSyllabusAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetAllSyllabusAsync();

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var data = ok.Value as ResponseObject;
            ((List<SyllabusDTO>)data!.Data!).Should().HaveCount(2);
        }

        // ✅ GET: list/active
        [Fact]
        public async Task GetAllActiveSyllabusAsync_ShouldReturnOk_WithData()
        {
            var fakeResponse = new ResponseObject("ok", "Active syllabuses", new List<SyllabusDTO>
            {
                new SyllabusDTO { Id = 1, Name = "OOP", IsActive = "true" }
            });

            _mockService.Setup(s => s.GetAllActiveSyllabusAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetAllActiveSyllabusAsync();

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var data = ok.Value as ResponseObject;
            ((List<SyllabusDTO>)data!.Data!).Should().HaveCount(1);
        }

        // ✅ PUT: Update - Success
        [Fact]
        public async Task UpdateSyllabusAsync_ShouldReturnOk_WhenValid()
        {
            var req = new UpdateSyllabusRequest(1, "Updated", "New Desc", 200, 40, "true");
            var fakeResponse = new ResponseObject("ok", "Updated successfully", req);

            _mockService.Setup(s => s.UpdateSyllabusAsync(req)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateSyllabusAsync(req);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
        }

        // ✅ PUT: Update - NotFound
        [Fact]
        public async Task UpdateSyllabusAsync_ShouldReturnNotFound_WhenMissing()
        {
            var req = new UpdateSyllabusRequest(99, "Missing", "None", 100, 10, "false");
            var fakeResponse = new ResponseObject("notFound", "Syllabus not found", null);

            _mockService.Setup(s => s.UpdateSyllabusAsync(req)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateSyllabusAsync(req);

            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be(404);
        }

        // ✅ PUT: Update - Conflict
        [Fact]
        public async Task UpdateSyllabusAsync_ShouldReturnConflict_WhenDuplicate()
        {
            var req = new UpdateSyllabusRequest(1, "OOP", "Dup", 300, 20, "true");
            var fakeResponse = new ResponseObject("conflict", "Name already exists", null);

            _mockService.Setup(s => s.UpdateSyllabusAsync(req)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateSyllabusAsync(req);

            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.StatusCode.Should().Be(409);
        }

        // ✅ PUT: Update - BadRequest
        [Fact]
        public async Task UpdateSyllabusAsync_ShouldReturnBadRequest_WhenInvalid()
        {
            var req = new UpdateSyllabusRequest(1, "", "", 0, 0, "false");
            var fakeResponse = new ResponseObject("badRequest", "Invalid data", null);

            _mockService.Setup(s => s.UpdateSyllabusAsync(req)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateSyllabusAsync(req);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.StatusCode.Should().Be(400);
        }

        // ✅ PUT: Update - Exception
        [Fact]
        public async Task UpdateSyllabusAsync_ShouldReturnServerError_OnException()
        {
            var req = new UpdateSyllabusRequest(1, "OOP", "Crash", 200, 20, "true");
            _mockService.Setup(s => s.UpdateSyllabusAsync(req)).ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.UpdateSyllabusAsync(req);

            var error = result as ObjectResult;
            error.Should().NotBeNull();
            error!.StatusCode.Should().Be(500);
        }
    }
}

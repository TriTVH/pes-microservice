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
    public class AdmissionTermControllerTest
    {
        private readonly Mock<IAdmissionTermService> _mockService;
        private readonly AdmissionTermController _controller;

        public AdmissionTermControllerTest()
        {
            _mockService = new Mock<IAdmissionTermService>();
            _controller = new AdmissionTermController(_mockService.Object);
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnBadRequest_WhenInvalidInput()
        {
            var request = new CreateAdmissionTermRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(10), new List<int> { 1 });
            var fakeResponse = new ResponseObject("badRequest", "Invalid date range", null);

            _mockService.Setup(s => s.CreateAdmissionTermAsync(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateAdmissionTermAsync(request);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.StatusCode.Should().Be(400);
        }

        // ✅ POST - conflict
        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnConflict_WhenOverlapDetected()
        {
            var request = new CreateAdmissionTermRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(10), new List<int> { 1 });
            var fakeResponse = new ResponseObject("conflict", "Term overlaps", null);

            _mockService.Setup(s => s.CreateAdmissionTermAsync(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateAdmissionTermAsync(request);

            result.Should().BeOfType<ConflictObjectResult>();
            (result as ConflictObjectResult)!.StatusCode.Should().Be(409);
        }

        // ✅ POST - ok
        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnOk_WhenSuccessful()
        {
            var request = new CreateAdmissionTermRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(10), new List<int> { 1 });
            var fakeResponse = new ResponseObject("ok", "Created successfully", new { Id = 1 });

            _mockService.Setup(s => s.CreateAdmissionTermAsync(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.CreateAdmissionTermAsync(request);

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.StatusCode.Should().Be(200);
        }

        // ✅ POST - 500 exception
        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturn500_WhenExceptionThrown()
        {
            var request = new CreateAdmissionTermRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(10), new List<int> { 1 });
            _mockService.Setup(s => s.CreateAdmissionTermAsync(request)).ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.CreateAdmissionTermAsync(request);

            result.Should().BeOfType<ObjectResult>();
            var res = result as ObjectResult;
            res!.StatusCode.Should().Be(500);
            (res.Value as ResponseObject)!.StatusResponseCode.Should().Contain("DB Error");
        }

        // ✅ GET - list
        [Fact]
        public async Task GetAdmissionTermsAsync_ShouldReturnOk()
        {
            var fakeResponse = new ResponseObject("ok", "Fetched list", new List<object>());
            _mockService.Setup(s => s.GetAllAdmissionTermsAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetAdmissionTermsAsync();

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.StatusCode.Should().Be(200);
        }

        // ✅ GET - active notFound
        [Fact]
        public async Task GetActiveAdmissionTermAsync_ShouldReturnNotFound_WhenNoActiveTerm()
        {
            var fakeResponse = new ResponseObject("notFound", "No active term", null);
            _mockService.Setup(s => s.GetActiveAdmissionTermAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetActiveAdmissionTermAsync();

            result.Should().BeOfType<NotFoundObjectResult>();
            (result as NotFoundObjectResult)!.StatusCode.Should().Be(404);
        }

        // ✅ GET - active ok
        [Fact]
        public async Task GetActiveAdmissionTermAsync_ShouldReturnOk_WhenFound()
        {
            var fakeResponse = new ResponseObject("ok", "Active term found", new { Id = 1 });
            _mockService.Setup(s => s.GetActiveAdmissionTermAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetActiveAdmissionTermAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        // ✅ GET - by id notFound
        [Fact]
        public async Task GetAdmissionTermByIdAsync_ShouldReturnNotFound_WhenNotFound()
        {
            var fakeResponse = new ResponseObject("notFound", "Term not found", null);
            _mockService.Setup(s => s.GetAdmissionTermById(2)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetAdmissionTermByIdAsync(2);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ✅ GET - by id ok
        [Fact]
        public async Task GetAdmissionTermByIdAsync_ShouldReturnOk_WhenFound()
        {
            var fakeResponse = new ResponseObject("ok", "Found", new { Id = 2 });
            _mockService.Setup(s => s.GetAdmissionTermById(2)).ReturnsAsync(fakeResponse);

            var result = await _controller.GetAdmissionTermByIdAsync(2);

            result.Should().BeOfType<OkObjectResult>();
        }

        // ✅ GET - comboBox
        [Fact]
        public async Task GetComboBoxAdmissionTermsAsync_ShouldReturnOk()
        {
            var fakeResponse = new ResponseObject("ok", "Combo box data", new List<object>());
            _mockService.Setup(s => s.GetComboBoxAdmissionTermsAsync()).ReturnsAsync(fakeResponse);

            var result = await _controller.GetComboBoxAdmissionTermsAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        // ✅ PUT - badRequest
        [Fact]
        public async Task UpdateAdmissionTermStatus_ShouldReturnBadRequest_WhenInvalidAction()
        {
            var request = new UpdateAdmissionTermActionRequest() 
            {
                Id = 1, 
                Action = "invalid"
            };
            var fakeResponse = new ResponseObject("badRequest", "Invalid action", null);

            _mockService.Setup(s => s.UpdateAdmissionTermStatusByAction(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateAdmissionTermStatus(request);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.StatusCode.Should().Be(400);
        }

        // ✅ PUT - conflict
        [Fact]
        public async Task UpdateAdmissionTermStatus_ShouldReturnConflict_WhenOverlapExists()
        {
            var request = new UpdateAdmissionTermActionRequest()
            {
                Id = 2,
                Action = "start"
            };
            var fakeResponse = new ResponseObject("conflict", "Overlapping term", null);

            _mockService.Setup(s => s.UpdateAdmissionTermStatusByAction(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateAdmissionTermStatus(request);

            result.Should().BeOfType<ConflictObjectResult>();
            (result as ConflictObjectResult)!.StatusCode.Should().Be(409);
        }

        // ✅ PUT - ok
        [Fact]
        public async Task UpdateAdmissionTermStatus_ShouldReturnOk_WhenSuccessful()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 3, Action = "end" };
            var fakeResponse = new ResponseObject("ok", "Status updated", new { Id = 3 });

            _mockService.Setup(s => s.UpdateAdmissionTermStatusByAction(request)).ReturnsAsync(fakeResponse);

            var result = await _controller.UpdateAdmissionTermStatus(request);

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.StatusCode.Should().Be(200);
        }
    }
}

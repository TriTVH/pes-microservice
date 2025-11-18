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
    public class AdmissionFormControllerTest
    {
        private readonly Mock<IAdmissionFormService> _mockService;
        private readonly AdmissionFormController _controller;

        public AdmissionFormControllerTest()
        {
            _mockService = new Mock<IAdmissionFormService>();
            _controller = new AdmissionFormController(_mockService.Object);
        }

        // ✅ TEST CASE 1: GetAdmissionFormsAsync trả về OK
        [Fact]
        public async Task GetAdmissionFormsAsync_ShouldReturnOk_WithValidResponse()
        {
            // Arrange
            int admissionTermId = 10;
            var fakeData = new List<object>
            {
                new { Id = 1, Status = "waiting_for_payment" },
                new { Id = 2, Status = "approved" }
            };

            var fakeResponse = new ResponseObject("ok", "Get forms successfully", fakeData);

            _mockService.Setup(s => s.GetAdmissionFormsByAdmissionTermIdAsync(admissionTermId))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.GetAdmissionFormsAsync(admissionTermId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult!.StatusCode.Should().Be(200);
            var response = okResult.Value as ResponseObject;
            response.Should().NotBeNull();
            response!.StatusResponseCode.Should().Be("ok");
            response.Data.Should().BeEquivalentTo(fakeData);

            _mockService.Verify(s => s.GetAdmissionFormsByAdmissionTermIdAsync(admissionTermId), Times.Once);
        }

        // ✅ TEST CASE 2: ChangeStatusAdmissionFormByAction trả về NotFound
        [Fact]
        public async Task ChangeStatusAdmissionFormByAction_ShouldReturnNotFound_WhenFormNotFound()
        {
            // Arrange
            var request = new UpdateAdmissionFormStatusRequest() 
            {
                Id = 1,
                Action = "approve"
            };
            var fakeResponse = new ResponseObject("notFound", "Form not found", null);

            _mockService.Setup(s => s.ChangeStatusOfAdmissionForm(request))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.ChangeStatusAdmissionFormByAction(request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;

            notFoundResult!.StatusCode.Should().Be(404);
            var response = notFoundResult.Value as ResponseObject;
            response!.StatusResponseCode.Should().Be("notFound");

            _mockService.Verify(s => s.ChangeStatusOfAdmissionForm(request), Times.Once);
        }

        // ✅ TEST CASE 3: ChangeStatusAdmissionFormByAction trả về OK khi thành công
        [Fact]
        public async Task ChangeStatusAdmissionFormByAction_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new UpdateAdmissionFormStatusRequest() 
            { 
                Id = 2,
                Action = "approve"
            };
            var fakeResponse = new ResponseObject("ok", "Status changed successfully", new { Id = 2, Status = "approved" });

            _mockService.Setup(s => s.ChangeStatusOfAdmissionForm(request))
                        .ReturnsAsync(fakeResponse);

            // Act
            var result = await _controller.ChangeStatusAdmissionFormByAction(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult!.StatusCode.Should().Be(200);
            var response = okResult.Value as ResponseObject;
            response.Should().NotBeNull();
            response!.StatusResponseCode.Should().Be("ok");
            response.Message.Should().Contain("successfully");

            _mockService.Verify(s => s.ChangeStatusOfAdmissionForm(request), Times.Once);
        }
    }
}

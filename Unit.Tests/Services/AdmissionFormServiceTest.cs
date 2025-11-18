using FluentAssertions;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services;
using SyllabusService.Domain.DTOs;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;

namespace Unit.Tests.Services
{
    public class AdmissionFormServiceTest
    {
        private readonly Mock<IAdmissionFormRepo> _admissionFormRepo;
        private readonly Mock<IParentClient> _parentClient;
        private readonly Mock<IAuthClient> _authClient;
        private readonly AdmissionFormService _service;

        public AdmissionFormServiceTest()
        {
            _admissionFormRepo = new Mock<IAdmissionFormRepo>();
            _parentClient = new Mock<IParentClient>();
            _authClient = new Mock<IAuthClient>();

            _service = new AdmissionFormService(
                _admissionFormRepo.Object,
                _parentClient.Object,
                _authClient.Object
            );
        }

        [Fact]
        public async Task GetAdmissionFormsByAdmissionTermIdAsync_ShouldReturnMappedDtos_WhenFormsExist()
        {
            // Arrange
            var forms = new List<AdmissionForm>
            {
                new AdmissionForm
                {
                    Id = 1,
                    StudentId = 101,
                    ParentAccountId = 201,
                    Status = "waiting_for_approve",
                    SubmittedDate = new DateTime(2025, 1, 1)
                }
            };

            var studentDto = new StudentDTO { Id = 101, Name = "John Doe" };
            var studentJson = JsonSerializer.SerializeToElement(studentDto);

            _admissionFormRepo.Setup(x => x.GetAdmissionFormsByAdmissionTermIdAsync(1))
                .ReturnsAsync(forms);

            _parentClient.Setup(x => x.GetStudentDtoById(101))
                .ReturnsAsync(new ResponseObjectFromAnotherClient("ok", "ok", studentJson));

            _authClient.Setup(x => x.GetParentProfileDto(201))
     .ReturnsAsync(new AccountDto(201, "a@mail.com", "Parent A", "PARENT", "ACTIVE", "0909123456", "123 Street", DateTime.UtcNow));


            // Act
            var result = await _service.GetAdmissionFormsByAdmissionTermIdAsync(1);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get All Admission Forms Successfully");
            result.Data.Should().BeAssignableTo<IEnumerable<AdmissionFormDto>>();

            var data = (IEnumerable<AdmissionFormDto>)result.Data!;
            data.Should().ContainSingle();
            data.First().Student.Should().NotBeNull();
            data.First().ParentAccount.Name.Should().Be("Parent A");
        }

        // ✅ 2️⃣ Test: GetAdmissionFormsByAdmissionTermIdAsync - no student JSON
        [Fact]
        public async Task GetAdmissionFormsByAdmissionTermIdAsync_ShouldHandleNullStudentData()
        {
            var forms = new List<AdmissionForm>
            {
                new AdmissionForm
                {
                    Id = 2,
                    StudentId = 102,
                    ParentAccountId = 202,
                    Status = "waiting_for_payment"
                }
            };

            _admissionFormRepo.Setup(x => x.GetAdmissionFormsByAdmissionTermIdAsync(It.IsAny<int>()))
                .ReturnsAsync(forms);

            // studentResult.Data không phải JsonElement
            _parentClient.Setup(x => x.GetStudentDtoById(102))
                .ReturnsAsync(new ResponseObjectFromAnotherClient("ok", "ok", "invalid-json-data"));

            _authClient.Setup(x => x.GetParentProfileDto(201))
                .ReturnsAsync(new AccountDto(201, "a@mail.com", "Parent A", "PARENT", "ACTIVE", "0909123456", "123 Street", DateTime.UtcNow));


            var result = await _service.GetAdmissionFormsByAdmissionTermIdAsync(2);

            result.StatusResponseCode.Should().Be("ok");
            var data = (IEnumerable<AdmissionFormDto>)result.Data!;
            data.First().Student.Should().BeNull(); // vì deserialize thất bại
        }

        // ✅ 3️⃣ Test: ChangeStatusOfAdmissionForm - form not found
        [Fact]
        public async Task ChangeStatusOfAdmissionForm_ShouldReturnNotFound_WhenFormDoesNotExist()
        {
            _admissionFormRepo.Setup(x => x.GetAdmissionFormByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AdmissionForm)null!);

            var request = new UpdateAdmissionFormStatusRequest { Id = 99, Action = "approve" };

            var result = await _service.ChangeStatusOfAdmissionForm(request);

            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("not found");
        }

        // ✅ 4️⃣ Test: ChangeStatusOfAdmissionForm - approve action
        [Fact]
        public async Task ChangeStatusOfAdmissionForm_ShouldApproveForm_WhenActionIsApprove()
        {
            var form = new AdmissionForm { Id = 1, Status = "waiting_for_approve" };

            _admissionFormRepo.Setup(x => x.GetAdmissionFormByIdAsync(1))
                .ReturnsAsync(form);

            _admissionFormRepo.Setup(x => x.UpdateAdmissionFormAsync(It.IsAny<AdmissionForm>()))
                .ReturnsAsync(1);

            var request = new UpdateAdmissionFormStatusRequest { Id = 1, Action = "approve" };

            var result = await _service.ChangeStatusOfAdmissionForm(request);

            result.StatusResponseCode.Should().Be("ok");
            form.Status.Should().Be("waiting_for_payment");
            form.ApprovedDate.Should().NotBeNull();
        }

        // ✅ 5️⃣ Test: ChangeStatusOfAdmissionForm - reject with custom reason
        [Fact]
        public async Task ChangeStatusOfAdmissionForm_ShouldRejectForm_WithCustomReason()
        {
            var form = new AdmissionForm { Id = 2, Status = "waiting_for_approve" };

            _admissionFormRepo.Setup(x => x.GetAdmissionFormByIdAsync(2))
                .ReturnsAsync(form);

            var request = new UpdateAdmissionFormStatusRequest
            {
                Id = 2,
                Action = "reject",
                CancelReason = "Incomplete documents"
            };

            var result = await _service.ChangeStatusOfAdmissionForm(request);

            result.StatusResponseCode.Should().Be("ok");
            form.Status.Should().Be("rejected");
            form.CancelReason.Should().Be("Incomplete documents");
        }

        // ✅ 6️⃣ Test: ChangeStatusOfAdmissionForm - reject with default reason
        [Fact]
        public async Task ChangeStatusOfAdmissionForm_ShouldRejectForm_WithDefaultReason()
        {
            var form = new AdmissionForm { Id = 3, Status = "waiting_for_approve" };

            _admissionFormRepo.Setup(x => x.GetAdmissionFormByIdAsync(3))
                .ReturnsAsync(form);

            var request = new UpdateAdmissionFormStatusRequest
            {
                Id = 3,
                Action = "reject",
                CancelReason = null
            };

            var result = await _service.ChangeStatusOfAdmissionForm(request);

            result.StatusResponseCode.Should().Be("ok");
            form.CancelReason.Should().Be("Rejected by educational manager.");
        }

        // ✅ 7️⃣ Test: ChangeStatusOfAdmissionForm - invalid action
        [Fact]
        public async Task ChangeStatusOfAdmissionForm_ShouldReturnBadRequest_WhenInvalidAction()
        {
            var form = new AdmissionForm { Id = 4, Status = "waiting_for_approve" };

            _admissionFormRepo.Setup(x => x.GetAdmissionFormByIdAsync(4))
                .ReturnsAsync(form);

            var request = new UpdateAdmissionFormStatusRequest
            {
                Id = 4,
                Action = "unknown"
            };

            var result = await _service.ChangeStatusOfAdmissionForm(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Invalid action");
        }
    }
}

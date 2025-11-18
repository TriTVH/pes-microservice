using FluentAssertions;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Tests.Services
{
    public class AdmissionTermServiceTest
    {
        private readonly AdmissionTermService _service;
        private readonly Mock<IAdmissionTermRepo> _admissionTermRepo;
        private readonly Mock<IClassRepository> _classRepo;


        public AdmissionTermServiceTest()
        {
            _admissionTermRepo = new Mock<IAdmissionTermRepo>();
            _classRepo = new Mock<IClassRepository>();

            _service = new AdmissionTermService(
               _admissionTermRepo.Object,
               _classRepo.Object
           );
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnBadRequest_WhenStartDateInPast()
        {
            var request = new CreateAdmissionTermRequest(
    DateTime.UtcNow.AddDays(-1),
    DateTime.UtcNow.AddDays(10),
    new List<int> { 1, 2 }
);


            var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Be("Start date cannot be in the past.");
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnBadRequest_WhenStartDateAfterEndDate()
        {
            var request = new CreateAdmissionTermRequest(
       DateTime.UtcNow.AddDays(10),
       DateTime.UtcNow.AddDays(5),
       new List<int> { 1, 2 }
   );

            var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Be("Start date must be earlier than end date.");
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnBadRequest_WhenNoClassIds()
        {
            var request = new CreateAdmissionTermRequest(
   DateTime.UtcNow.AddDays(1),
   DateTime.UtcNow.AddDays(5),
   new List<int>());


        var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Be("At least one class ID must be provided.");
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnConflict_WhenSomeClassesNotExist()
        {
            var request = new CreateAdmissionTermRequest(
   DateTime.UtcNow.AddDays(1),
   DateTime.UtcNow.AddDays(5),
   new List<int> { 1 ,2 ,3 });

            _classRepo.Setup(x => x.GetClassesByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Class> { new Class { Id = 1 }, new Class { Id = 2 } });

            var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("conflict");
            result.Message.Should().Contain("recently deleted");
        }

        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnConflict_WhenOverlappingTermExists()
        {
            var request = new CreateAdmissionTermRequest(
DateTime.UtcNow.AddDays(1),
DateTime.UtcNow.AddDays(5),
new List<int> { 1 });

            _classRepo.Setup(x => x.GetClassesByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Class> { new Class { Id = 1 } });

            _admissionTermRepo.Setup(x => x.GetOverlappingTermAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new AdmissionTerm
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(2)
                });

            var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("conflict");
            result.Message.Should().Contain("overlap");
        }
        [Fact]
        public async Task CreateAdmissionTermAsync_ShouldReturnOk_WhenValidRequest()
        {
            var request = new CreateAdmissionTermRequest(
   DateTime.UtcNow.AddDays(1),
   DateTime.UtcNow.AddDays(5),
   new List<int> { 1, 2 });

            var classes = new List<Class>
            {
                new Class { Id = 1 },
                new Class { Id = 2 }
            };

            _classRepo.Setup(x => x.GetClassesByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(classes);

            _admissionTermRepo.Setup(x => x.GetOverlappingTermAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync((AdmissionTerm)null);

            _admissionTermRepo.Setup(x => x.CreateAdmissionTermAsync(It.IsAny<AdmissionTerm>()))
                .ReturnsAsync(1);

            var result = await _service.CreateAdmissionTermAsync(request);

            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Create admission term successfully");

            _admissionTermRepo.Verify(x => x.CreateAdmissionTermAsync(It.IsAny<AdmissionTerm>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnNotFound_WhenTermDoesNotExist()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action="start" };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AdmissionTerm)null);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("not found");
        }
        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnBadRequest_WhenStartNonInactive()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "start" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "active",
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Only inactive terms can be started");
        }
        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnConflict_WhenOverlapDetected()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "start" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "inactive",
                EndDate = DateTime.UtcNow.AddDays(3)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            _admissionTermRepo.Setup(x => x.GetOverlappingTermAsyncExceptId(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(new AdmissionTerm
                {
                    Id = 2,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(1)
                });

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("conflict");
            result.Message.Should().Contain("overlaps");
        }

        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnOk_WhenStartValid()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "start" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "inactive",
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            _admissionTermRepo.Setup(x => x.GetOverlappingTermAsyncExceptId(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync((AdmissionTerm)null);

            _admissionTermRepo.Setup(x => x.UpdateAdmissionTermAsync(It.IsAny<AdmissionTerm>()))
                .ReturnsAsync(1);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("started successfully");
            term.Status.Should().Be("active");
        }

        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnBadRequest_WhenEndNonActive()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "end" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "inactive",
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Only active terms can be ended");
        }

        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnOk_WhenEndValid()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "end" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "active",
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            _admissionTermRepo.Setup(x => x.UpdateAdmissionTermAsync(It.IsAny<AdmissionTerm>()))
                .ReturnsAsync(1);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("ok");
            term.Status.Should().Be("blocked");
        }

        [Fact]
        public async Task UpdateAdmissionTermStatusByAction_ShouldReturnBadRequest_WhenInvalidAction()
        {
            var request = new UpdateAdmissionTermActionRequest() { Id = 1, Action = "pause" };

            var term = new AdmissionTerm
            {
                Id = 1,
                Status = "inactive",
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            var result = await _service.UpdateAdmissionTermStatusByAction(request);

            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Invalid action");
        }

        [Fact]
        public async Task GetAdmissionTermById_ShouldReturnNotFound_WhenTermDoesNotExist()
        {
            // Arrange
            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AdmissionTerm)null);

            // Act
            var result = await _service.GetAdmissionTermById(10);

            // Assert
            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task GetAdmissionTermById_ShouldReturnOk_WhenTermExists()
        {
            // Arrange
            var term = new AdmissionTerm
            {
                Id = 1,
                AcdemicYear = 2025,
                MaxNumberRegistration = 100,
                CurrentRegisteredStudents = 50,
                NumberOfClasses = 4,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = "active"
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermByIdAsync(1))
                .ReturnsAsync(term);

            // Act
            var result = await _service.GetAdmissionTermById(1);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("successfully");
            result.Data.Should().NotBeNull();
        }
        [Fact]
        public async Task GetComboBoxAdmissionTermsAsync_ShouldReturnOk_WhenDataExists()
        {
            // Arrange
            var terms = new List<AdmissionTerm>
            {
                new AdmissionTerm
                {
                    Id = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 6, 1)
                },
                new AdmissionTerm
                {
                    Id = 2,
                    StartDate = new DateTime(2025, 7, 1),
                    EndDate = new DateTime(2025, 12, 31)
                }
            };

            _admissionTermRepo.Setup(x => x.GetPrioritizedAdmissionTermsAsync())
                .ReturnsAsync(terms);

            // Act
            var result = await _service.GetComboBoxAdmissionTermsAsync();

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Combo Box");
            result.Data.Should().BeAssignableTo<IEnumerable<ComboItemAdmissionTerm>>();
            result.Data.Should().NotBeNull();
        }
        [Fact]
        public async Task GetAllAdmissionTermsAsync_ShouldReturnOk_WhenDataExists()
        {
            // Arrange
            var terms = new List<AdmissionTerm>
            {
                new AdmissionTerm
                {
                    Id = 1,
                    AcdemicYear = 2025,
                    MaxNumberRegistration = 100,
                    CurrentRegisteredStudents = 30,
                    NumberOfClasses = 3,
                    StartDate = new DateTime(2025, 1, 10),
                    EndDate = new DateTime(2025, 5, 10),
                    Status = "active",
                    Classes = new List<Class>
                    {
                        new Class
                        {
                            Id = 101,
                            Name = "PRN222",
                            NumberOfWeeks = 10,
                            NumberStudent = 25,
                            AcademicYear = 2025,
                            StartDate = new DateOnly(2025, 1, 10),
                            Status = "active",
                            Syllabus = new Syllabus
                            {
                                Id = 11,
                                Cost = 500000,
                                Name = "PRN222"
                            }
                        },
                        new Class
                        {
                            Id = 102,
                            Name = "SWP391",
                            NumberOfWeeks = 12,
                            NumberStudent = 20,
                            AcademicYear = 2025,
                            StartDate = new DateOnly(2025, 2, 1),
                            Status = "inactive",
                            Syllabus = new Syllabus
                            {
                                Id = 12,
                                Cost = 600000,
                                Name = "SWP391"
                            }
                        }
                    }
                }
            };

            _admissionTermRepo.Setup(x => x.GetAdmissionTermsAsync())
                .ReturnsAsync(terms);

            // Act
            var result = await _service.GetAllAdmissionTermsAsync();

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get all admission terms successfully");
            result.Data.Should().BeAssignableTo<IEnumerable<AdmissionTermDto>>();

            var list = result.Data as IEnumerable<AdmissionTermDto>;
            list.Should().HaveCount(1);

            var firstTerm = list!.First();
            firstTerm.Id.Should().Be(1);
            firstTerm.ClassDtos.Should().HaveCount(2);
            firstTerm.ClassDtos.First().Cost.Should().Be(500000);
        }
        [Fact]
        public async Task GetActiveAdmissionTermAsync_ShouldReturnNotFound_WhenNoActiveTermExists()
        {
            // Arrange
            _admissionTermRepo.Setup(x => x.GetActiveAdmissionTerm())
                .ReturnsAsync((AdmissionTerm)null);

            // Act
            var result = await _service.GetActiveAdmissionTermAsync();

            // Assert
            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("no active admission term");
            result.Data.Should().BeNull();
        }
        [Fact]
        public async Task GetActiveAdmissionTermAsync_ShouldReturnOk_WhenActiveTermExists()
        {
            // Arrange
            var activeTerm = new AdmissionTerm
            {
                Id = 1,
                AcdemicYear = 2025,
                MaxNumberRegistration = 120,
                CurrentRegisteredStudents = 60,
                NumberOfClasses = 2,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 6, 30),
                Status = "active",
                Classes = new List<Class>
        {
            new Class
            {
                Id = 101,
                Name = "PRN222",
                NumberOfWeeks = 10,
                NumberStudent = 25,
                AcademicYear = 2025,
                StartDate = new DateOnly(2025, 1, 1),
                Status = "active",
                Syllabus = new Syllabus { Id = 11, Cost = 500000 },
                PatternActivities = new List<PatternActivity>
                {
                    new PatternActivity
                    {
                        Id = 1,
                        DayOfWeek = "Monday",
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 0)
                    },
                    new PatternActivity
                    {
                        Id = 2,
                        DayOfWeek = "Wednesday",
                        StartTime = new TimeOnly(13, 0),
                        EndTime = new TimeOnly(15, 0)
                    }
                }
            }
        }
            };

            _admissionTermRepo.Setup(x => x.GetActiveAdmissionTerm())
                .ReturnsAsync(activeTerm);

            // Act
            var result = await _service.GetActiveAdmissionTermAsync();

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get active admission terms successfully");
            result.Data.Should().BeAssignableTo<AdmissionTermDto>();

            var dto = (AdmissionTermDto)result.Data!;
            dto.Id.Should().Be(1);
            dto.Status.Should().Be("active");
            dto.ClassDtos.Should().HaveCount(1);
            dto.ClassDtos.First().PatternActivitiesDTO.Should().HaveCount(2);
            dto.ClassDtos.First().PatternActivitiesDTO.First().DayOfWeek.Should().Be("Monday");
        }
    }
}

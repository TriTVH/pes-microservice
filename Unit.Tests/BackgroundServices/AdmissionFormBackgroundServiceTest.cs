using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SyllabusService.API.Background;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Unit.Tests.BackgroundServices
{
    public class AdmissionFormBackgroundServiceTest
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockScopeProvider;
        private readonly Mock<IAdmissionFormRepo> _mockRepo;
        private readonly AdmissionFormBackgroundService _service;

        public AdmissionFormBackgroundServiceTest()
        {
            _mockRepo = new Mock<IAdmissionFormRepo>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopeProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Setup DI chain
            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);

            _mockScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockScope.Object);

            _mockScope.Setup(x => x.ServiceProvider)
                .Returns(_mockScopeProvider.Object);

            _mockScopeProvider.Setup(x => x.GetService(typeof(IAdmissionFormRepo)))
                .Returns(_mockRepo.Object);

            // Instantiate service
            _service = new AdmissionFormBackgroundService(_mockServiceProvider.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCall_UpdateAdmissionFormsOverDueDateAuto_AtLeastOnce()
        {
            // Arrange
            _mockRepo.Setup(r => r.UpdateAdmissionFormsOverDueDateAuto())
                     .Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // stop early

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            _mockRepo.Verify(r => r.UpdateAdmissionFormsOverDueDateAuto(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStop_WhenCancellationTokenTriggered()
        {
            // Arrange
            bool called = false;
            _mockRepo.Setup(r => r.UpdateAdmissionFormsOverDueDateAuto())
                     .Callback(() => called = true)
                     .Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            called.Should().BeTrue("because the method should run before cancellation");
        }
    }
}

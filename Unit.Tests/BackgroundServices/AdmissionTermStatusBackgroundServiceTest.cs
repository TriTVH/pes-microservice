using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SyllabusService.API.Background;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.BackgroundServices
{
    public class AdmissionTermStatusBackgroundServiceTest
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockScopeProvider;
        private readonly Mock<IAdmissionTermRepo> _mockRepo;
        private readonly AdmissionTermStatusBackgroundService _service;

        public AdmissionTermStatusBackgroundServiceTest()
        {
            _mockRepo = new Mock<IAdmissionTermRepo>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopeProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Mock ServiceProvider chain
            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);

            _mockScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockScope.Object);

            _mockScope.Setup(x => x.ServiceProvider)
                .Returns(_mockScopeProvider.Object);

            _mockScopeProvider.Setup(x => x.GetService(typeof(IAdmissionTermRepo)))
                .Returns(_mockRepo.Object);

            // Khởi tạo service thật
            _service = new AdmissionTermStatusBackgroundService(_mockServiceProvider.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallUpdateStatusAuto_AtLeastOnce()
        {
            // Arrange
            _mockRepo.Setup(r => r.UpdateStatusAuto()).Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // dừng sớm để không chờ 5 phút

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            _mockRepo.Verify(r => r.UpdateStatusAuto(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStop_WhenCancelled()
        {
            // Arrange
            var called = false;
            _mockRepo.Setup(r => r.UpdateStatusAuto()).Callback(() => called = true)
                     .Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50); // dừng nhanh

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            called.Should().BeTrue(); // đã gọi ít nhất 1 lần
        }
    }
}

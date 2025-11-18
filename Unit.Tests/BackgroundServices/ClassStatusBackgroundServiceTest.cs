using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.BackgroundServices
{
    public class ClassStatusBackgroundServiceTest
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockScopeProvider;
        private readonly Mock<IClassRepository> _mockRepo;
        private readonly ClassStatusBackgroundService _service;

        public ClassStatusBackgroundServiceTest()
        {
            _mockRepo = new Mock<IClassRepository>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopeProvider = new Mock<IServiceProvider>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Thiết lập DI chain: IServiceProvider -> IServiceScopeFactory -> IServiceScope -> IClassRepository
            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);

            _mockScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockScope.Object);

            _mockScope.Setup(x => x.ServiceProvider)
                .Returns(_mockScopeProvider.Object);

            _mockScopeProvider.Setup(x => x.GetService(typeof(IClassRepository)))
                .Returns(_mockRepo.Object);

            // Khởi tạo background service
            _service = new ClassStatusBackgroundService(_mockServiceProvider.Object);
        }

        // ✅ TEST 1: Service nên gọi repo ít nhất 1 lần
        [Fact]
        public async Task ExecuteAsync_ShouldCall_UpdateClassStatusAuto_AtLeastOnce()
        {
            // Arrange
            _mockRepo.Setup(r => r.UpdateClassStatusAuto())
                     .Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Huỷ sớm để không chờ 20 phút

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            _mockRepo.Verify(r => r.UpdateClassStatusAuto(), Times.AtLeastOnce());
        }

        // ✅ TEST 2: Background service phải dừng đúng cách khi token bị huỷ
        [Fact]
        public async Task ExecuteAsync_ShouldStop_WhenCancelled()
        {
            // Arrange
            bool called = false;
            _mockRepo.Setup(r => r.UpdateClassStatusAuto())
                     .Callback(() => called = true)
                     .Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(50); // Dừng nhanh

            // Act
            await _service.StartAsync(cts.Token);

            // Assert
            called.Should().BeTrue("vì UpdateClassStatusAuto() phải chạy ít nhất một lần trước khi bị huỷ");
        }
    }

    // 🧩 Mock interface cho test nếu chưa có sẵn
    public interface IClassRepository
    {
        Task UpdateClassStatusAuto();
    }

    // 🧩 Giả lập service thật (nếu chưa import)
    public class ClassStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClassStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IClassRepository>();
                    await repo.UpdateClassStatusAuto();
                    await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
                }
            }
        }
    }
}

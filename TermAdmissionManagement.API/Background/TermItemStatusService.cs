using System;
using TermAdmissionManagement.Infrastructure.DBContext;

namespace TermAdmissionManagement.API.Background
{
    public class TermItemStatusService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TermItemStatusService> _logger;

        public TermItemStatusService(IServiceProvider serviceProvider, ILogger<TermItemStatusService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;

                    // lấy các TermItem thuộc AdmissionTerm có StartDate <= now và TermItem đang Pending
                    var items = await db.TermItems
                        .Include(t => t.AdmissionTerm)
                        .Where(t => t.AdmissionTerm.StartDate <= now && t.Status == TermItemStatus.Pending)
                        .ToListAsync(stoppingToken);

                    foreach (var item in items)
                    {
                        item.Status = TermItemStatus.Processing;
                        _logger.LogInformation("TermItem {ItemId} chuyển sang Processing (Term {TermId})", item.Id, item.AdmissionTermId);
                    }

                    if (items.Any())
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Term Item");
                }

                // chờ 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

using SyllabusService.Infrastructure.Repositories.IRepositories;

namespace SyllabusService.API.Background
{
    public class AdmissionTermStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AdmissionTermStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IAdmissionTermRepo>();
                    await repo.UpdateStatusAuto();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // kiểm tra mỗi 1 phut
            }
        }
    }
}

using SyllabusService.Infrastructure.Repositories.IRepositories;

namespace SyllabusService.API.Background
{
    public class AdmissionFormBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AdmissionFormBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IAdmissionFormRepo>();
                    await repo.UpdateAdmissionFormsOverDueDateAuto();
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // kiểm tra mỗi 2 phut
            }
        }
        }
}

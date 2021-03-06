
namespace HyperVLauncher.Services.Monitor
{
    public class Worker : BackgroundService
    {
        private Task? _monitorTask;
        private MonitorService? _monitorService;

        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _monitorTask = Task.Run(async () =>
            {
                _monitorService = ActivatorUtilities
                    .CreateInstance<MonitorService>(
                        _serviceProvider,
                        cancellationToken);

                await _monitorService.Run();

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_monitorTask is not null)
            {
                await _monitorTask;
            }

            if (_monitorService is not null)
            {
                await _monitorService.Stop();
            }
        }
    }
}

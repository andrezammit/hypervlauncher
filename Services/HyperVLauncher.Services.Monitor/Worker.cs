namespace HyperVLauncher.Services.Monitor
{
    public class Worker : BackgroundService
    {
        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var monitorService = new MonitorService(cancellationToken);
            await monitorService.Run();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Monitor stopped.");

            return base.StopAsync(cancellationToken);
        }
    }
}
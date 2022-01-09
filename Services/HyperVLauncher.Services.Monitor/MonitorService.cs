namespace HyperVLauncher.Services.Monitor
{
    public class MonitorService
    {
        private readonly CancellationToken _cancellationToken;

        public MonitorService(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public async Task Run()
        {
            Console.WriteLine("Monitor started.");

            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, _cancellationToken);
            }
        }
    }
}
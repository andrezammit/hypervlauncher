using HyperVLauncher.Services.Monitor;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    });

if (Environment.UserInteractive)
{
    await hostBuilder
        .RunConsoleAsync();
}
else
{
    await hostBuilder
        .UseWindowsService()
        .Build()
        .RunAsync();
}

Console.WriteLine("Stopped.");
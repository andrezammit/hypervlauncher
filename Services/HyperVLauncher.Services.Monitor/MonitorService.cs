using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Services.Monitor
{
    public class MonitorService
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly CancellationToken _cancellationToken;

        public MonitorService(
            IHyperVProvider hyperVProvider,
            ISettingsProvider settingsProvider,
            CancellationToken cancellationToken)
        {
            _hyperVProvider = hyperVProvider;
            _settingsProvider = settingsProvider;
            _cancellationToken = cancellationToken;
        }

        public Task Run()
        {
            _hyperVProvider.StartVirtualMachineMonitor(_cancellationToken);

            _hyperVProvider.OnNewVirtualMachine = OnNewVirtualMachine;

            return Task.CompletedTask;
        }

        public async Task OnNewVirtualMachine(VirtualMachine vm)
        {
            Tracer.Info($"New Virtual Machine detected: {vm.Id} - {vm.Name}");

            var shortcut = AppSettings.CreateShortcut(vm.Name, vm.Id);

            var appSettings = await _settingsProvider.Get(true);
            appSettings.Shortcuts.Add(shortcut);

            await _settingsProvider.Save();

            Tracer.Info($"New Shortcut created: {shortcut.Id}");
        }
    }
}

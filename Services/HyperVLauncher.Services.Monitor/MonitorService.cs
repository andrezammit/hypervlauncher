using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Services.Monitor
{
    public class MonitorService
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ITrayIpcProvider _trayIpcProvider;
        private readonly IShortcutProvider _shortcutProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMonitorIpcProvider _monitorIpcProvider;
        private readonly IRdpLauncherProvider _rdpLauncherProvider;

        private Task? _ipcProxy;
        private Task? _ipcProcessor;

        private readonly CancellationToken _cancellationToken;

        public MonitorService(
            IHyperVProvider hyperVProvider,
            ITrayIpcProvider trayIpcProvider,
            ISettingsProvider settingsProvider,
            IShortcutProvider shortcutProvider,
            IMonitorIpcProvider monitorIpcProvider,
            IRdpLauncherProvider rdpLauncherProvider,
            CancellationToken cancellationToken)
        {
            _hyperVProvider = hyperVProvider;
            _trayIpcProvider = trayIpcProvider;
            _shortcutProvider = shortcutProvider;
            _settingsProvider = settingsProvider;
            _monitorIpcProvider = monitorIpcProvider;
            _rdpLauncherProvider = rdpLauncherProvider;

            _cancellationToken = cancellationToken;
        }

        public async Task Run()
        {
            await CheckForInvalidShortcuts();

            await _rdpLauncherProvider.Start();

            _hyperVProvider.StartVirtualMachineCreatedMonitor(_cancellationToken);
            _hyperVProvider.StartVirtualMachineDeletedMonitor(_cancellationToken);

            _hyperVProvider.OnVirtualMachineCreated = OnVirtualMachineCreated;
            _hyperVProvider.OnVirtualMachineDeleted = OnVirtualMachineDeleted;

            _ipcProxy = Task.Run(() => _monitorIpcProvider.RunIpcProxy(_cancellationToken));
            _ipcProcessor = Task.Run(() => ProcessIpcMessages(_cancellationToken));

            Tracer.Info("Monitor service initialization done.");
        }

        public async Task Stop()
        {
            try
            {
                if (_ipcProcessor is not null)
                {
                    await _ipcProcessor;
                }

                if (_ipcProxy is not null)
                {
                    await _ipcProxy;
                }

                await _rdpLauncherProvider.Stop();
            }
            catch (Exception ex)
            {
                Tracer.Debug("Failed to stop gracefully.", ex);
            }
        }

        private async Task CheckForInvalidShortcuts()
        {
            var appSettings = await _settingsProvider.Get(true);

            if (!appSettings.AutoDeleteShortcuts)
            {
                return;
            }

            Tracer.Info("Checking for invalid shortcuts...");

            var vmList = _hyperVProvider.GetVirtualMachineList().ToList();

            var vmIdsToDelete = new HashSet<string>();

            foreach (var shortcut in appSettings.Shortcuts)
            {
                if (vmList.FirstOrDefault(x => x.Id == shortcut.VmId) is null)
                {
                    vmIdsToDelete.Add(shortcut.VmId);
                }
            }

            foreach (var vmIdToDelete in vmIdsToDelete)
            {
                await _settingsProvider.DeleteVirtualMachineShortcuts(
                    vmIdToDelete, 
                    _trayIpcProvider, 
                    _shortcutProvider);
            }
        }

        public async Task OnVirtualMachineCreated(VirtualMachine vm)
        {
            Tracer.Info($"New Virtual Machine detected: {vm.Id} - {vm.Name}");

            var appSettings = await _settingsProvider.Get(true);

            if (appSettings.AutoCreateShortcuts)
            {
                await _settingsProvider.ProcessCreateShortcut(
                    vm.Id,
                    vm.Name,
                    _trayIpcProvider,
                    _shortcutProvider);
            }
            else if (appSettings.NotifyOnNewVm)
            {
                await _trayIpcProvider.SendShowShortcutPromptNotif(
                    vm.Id,
                    vm.Name);
            }
        }

        public async Task OnVirtualMachineDeleted(VirtualMachine vm)
        {
            Tracer.Info($"Deleted Virtual Machine detected: {vm.Id} - {vm.Name}");

            var appSettings = await _settingsProvider.Get(true);

            if (appSettings.AutoDeleteShortcuts)
            {
                await _settingsProvider.DeleteVirtualMachineShortcuts(
                    vm.Id,
                    _trayIpcProvider,
                    _shortcutProvider);
            }
        }

        private Task ProcessIpcMessages(CancellationToken cancellationToken)
        {
            Tracer.Info("Starting processing of IPC messages...");

            try
            {
                foreach (var ipcMessage in _monitorIpcProvider.ReadMessages(cancellationToken))
                {
                    switch (ipcMessage.IpcCommand)
                    {
                        case IpcCommand.ReloadSettings:
                            _rdpLauncherProvider.RefreshListeners();
                            break;

                        default:
                            throw new InvalidDataException($"Invalid IPC command: {ipcMessage.IpcCommand}");
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Error("Error while processing IPC messages.", ex);

                //throw;
            }

            Tracer.Info("Stopped processing of IPC messages.");

            return Task.CompletedTask;
        }
    }
}

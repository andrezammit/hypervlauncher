using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Providers.Settings;

int vmConnectProcessId = 0;

string? vmName = null;

Task? ipcProcessor = null;
CancellationTokenSource cancellationTokenSource = new();

try
{
    var pathProvider = new PathProvider(GeneralConstants.ProfileName);
    pathProvider.CreateDirectories();

    TracingProvider.Init(pathProvider.GetTracingPath(), "LaunchPad");

    if (args.Length < 1)
    {
        Tracer.Error("Shortcut ID parameter could not be read.");
        return;
    }

    var shortcutId = args[0];
    Console.Title = shortcutId;

    using var instanceMutex = GenericHelpers.TakeInstanceMutex($"{GeneralConstants.LaunchPadMutexName}_{shortcutId}");

    Tracer.Debug($"Shortcut ID: {shortcutId}");

    var settingsProvider = new SettingsProvider(pathProvider);
    var trayIpcProvider = new IpcProvider(GeneralConstants.TrayIpcPipeName);
    var launchPadIpcProvider = new IpcProvider(GeneralConstants.LaunchPadIpcPipeName);

    ipcProcessor = Task.Run(
        () => ProcessIpcMessages(launchPadIpcProvider, cancellationTokenSource.Token));

    var appSettings = await settingsProvider.Get();
    var shortcut = appSettings.Shortcuts.FirstOrDefault(x => x.Id == shortcutId);

    if (shortcut is null)
    {
        throw new InvalidDataException($"Shortcut with ID {shortcutId} not found in settings.");
    }

    var hyperVProvider = new HyperVProvider();
    vmName = hyperVProvider.GetVirtualMachineName(shortcut.VmId);

    var vmState = hyperVProvider.GetVirtualMachineState(shortcut.VmId);

    if (vmState == VmState.Saved || vmState == VmState.Stopped)
    {
        Tracer.Info($"Starting {vmName}...");

        hyperVProvider.StartVirtualMachine(shortcut.VmId);
    }

    Tracer.Info($"Connecting to {vmName}...");

    using var process = hyperVProvider.ConnectVirtualMachine(shortcut.VmId);

    if (process is not null)
    {
        vmConnectProcessId = process.Id;

        Tracer.Info($"Waiting for {process.ProcessName} ({vmConnectProcessId}) to close...");

        await process.WaitForExitAsync();

        Tracer.Info($"{process.ProcessName} ({process.Id}) closed. Handling any further actions...");

        await HandleShortcutExitBehaviour(hyperVProvider, trayIpcProvider, shortcut);
    }

    cancellationTokenSource.Cancel();

    await ipcProcessor;

    Tracer.Debug("Closing LaunchPad app...");
}
catch (Exception ex)
{
    Tracer.Error("Failed to start shortcut.", ex);
}

async Task HandleShortcutExitBehaviour(
    IHyperVProvider hyperVProvider, 
    IIpcProvider ipcProvider,
    Shortcut shortcut)
{
    switch (shortcut.CloseAction)
    {
        case HyperVLauncher.Contracts.Enums.CloseAction.Pause:
            Tracer.Info($"Pausing {vmName}...");

            await ipcProvider.SendShowTrayMessage("Virtual Machine State Change", $"Pausing {shortcut.Name}...");
            hyperVProvider.PauseVirtualMachine(shortcut.VmId);

            Tracer.Info($"{vmName} paused.");

            break;

        case HyperVLauncher.Contracts.Enums.CloseAction.Shutdown:
            Tracer.Info($"Shutting down {vmName}...");

            await ipcProvider.SendShowTrayMessage("Virtual Machine State Change", $"Shutting down {shortcut.Name}...");
            hyperVProvider.ShutdownVirtualMachine(shortcut.VmId);

            Tracer.Info($"{vmName} shut down.");

            break;

        default:
            break;
    }
}

async Task ProcessIpcMessages(IIpcProvider ipcProvider, CancellationToken cancellationToken)
{
    try
    {
        await foreach (var ipcMessage in ipcProvider.ReadMessages(cancellationToken))
        {
            switch (ipcMessage.IpcCommand)
            {
                case IpcCommand.BringToFront:

                    try
                    {
                        var process = Process.GetProcessById(vmConnectProcessId);
                        process.BringToFront();
                    }
                    catch
                    {
                        // Swallow.
                    }

                    break;

                default:
                    throw new InvalidDataException($"Invalid IPC command: {ipcMessage.IpcCommand}");
            }
        }
    }
    catch (OperationCanceledException)
    {
        // Swallow.
    }
    catch (Exception ex)
    {
        Tracer.Error("Error while processing IPC messages.", ex);

        throw;
    }
}

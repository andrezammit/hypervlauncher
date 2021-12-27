using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Settings;

if (args.Length < 1)
{
    Console.WriteLine("Shortcut ID parameter could not be read.");
}

var shortcutId = args[0];

var profilePath = PathProvider.GetProfileFolder();
Directory.CreateDirectory(profilePath);

var pathProvider = new PathProvider(profilePath);
var settingsProvider = new SettingsProvider(pathProvider);
var ipcProvider = new IpcProvider(GeneralConstants.IpcPipeName);

var appSettings = await settingsProvider.Get();
var shortcut = appSettings.Shortcuts.FirstOrDefault(x => x.Id == shortcutId);

if (shortcut is null)
{
    throw new InvalidDataException($"Shortcut with ID {shortcutId} not found in settings.");
}

var hyperVProvider = new HyperVProvider();

Console.WriteLine($"Starting virtual machine {hyperVProvider.GetVmName(shortcut.VmId)}...");

hyperVProvider.StartVirtualMachine(shortcut.VmId);
using var process = hyperVProvider.ConnectVirtualMachine(shortcut.VmId);

if (process is not null)
{
    await process.WaitForExitAsync();

    await HandleShortcutExitBehaviour(hyperVProvider, ipcProvider, shortcut);
}

static async Task HandleShortcutExitBehaviour(
    IHyperVProvider hyperVProvider, 
    IIpcProvider ipcProvider,
    Shortcut shortcut)
{
    switch (shortcut.CloseAction)
    {
        case HyperVLauncher.Contracts.Enums.CloseAction.Pause:
            await ipcProvider.SendShowTrayMessage("Virtual Machine State Change", $"Pausing {shortcut.Name}...");
            hyperVProvider.PauseVirtualMachine(shortcut.VmId);
            
            break;

        case HyperVLauncher.Contracts.Enums.CloseAction.Shutdown:
            await ipcProvider.SendShowTrayMessage("Virtual Machine State Change", $"Shutting down {shortcut.Name}...");
            hyperVProvider.ShutdownVirtualMachine(shortcut.VmId);

            break;

        default:
            break;
    }
}

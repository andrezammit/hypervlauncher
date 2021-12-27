using System;
using System.IO;
using System.Linq;

using HyperVLauncher.Providers.Path;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Settings;
using HyperVLauncher.Contracts.Interfaces;

if (args.Length < 1)
{
    Console.WriteLine("Shortcut ID parameter could not be read.");
}

var shortcutId = args[0];

var profilePath = PathProvider.GetProfileFolder();
Directory.CreateDirectory(profilePath);

var pathProvider = new PathProvider(profilePath);
var settingsProvider = new SettingsProvider(pathProvider);

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

    HandleShortcutExitBehaviour(hyperVProvider, shortcut);
}

static void HandleShortcutExitBehaviour(IHyperVProvider hyperVProvider, Shortcut shortcut)
{
    switch (shortcut.CloseAction)
    {
        case HyperVLauncher.Contracts.Enums.CloseAction.Pause:
            hyperVProvider.PauseVirtualMachine(shortcut.VmId);
            break;

        case HyperVLauncher.Contracts.Enums.CloseAction.Shutdown:
            hyperVProvider.ShutdownVirtualMachine(shortcut.VmId);
            break;

        default:
            break;
    }
}

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

var appSettings = await settingsProvider.Get();
var shortcut = appSettings.Shortcuts.FirstOrDefault(x => x.Id == shortcutId);

if (shortcut is null)
{
    throw new InvalidDataException($"Shortcut with ID {shortcutId} not found in settings.");
}

var hyperVProvider = new HyperVProvider();

Console.WriteLine($"Starting virtual machine {hyperVProvider.GetVmName(shortcut.VmId)}...");

hyperVProvider.StartVirtualMachine(shortcut.VmId);
hyperVProvider.ConnectVirtualMachine(shortcut.VmId);
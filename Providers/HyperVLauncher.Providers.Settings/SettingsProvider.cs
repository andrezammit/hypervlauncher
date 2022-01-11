using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;
using System.Linq;

namespace HyperVLauncher.Providers.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        private AppSettings? _settings;
        private readonly IPathProvider _pathProvider;

        public SettingsProvider(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        public async Task<AppSettings> Get(bool forceReload = false)
        {
            if (_settings == null || forceReload)
            {
                Tracer.Debug("Loading settings from disk...");

                var settingsFilePath = _pathProvider.GetSettingsFilePath();
                
                Tracer.Debug($"Settings file path: {settingsFilePath}");

                try
                {
                    var appSettingsJson = await File
                        .ReadAllTextAsync(settingsFilePath)
                        .ConfigureAwait(false);

                    _settings = JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);
                }
                catch (Exception ex)
                {
                    Tracer.Debug("Failed to load settings.", ex);
                }

                if (_settings == null)
                {
                    _settings = new AppSettings();
                }

                Tracer.Debug("Settings loaded from disk.");
            }

            return _settings;
        }

        public async Task Save()
        {
            Tracer.Debug("Saving settings to disk...");

            if (_settings == null)
            {
                return;
            }

            var settingsFilePath = _pathProvider.GetSettingsFilePath();
            
            Tracer.Debug($"Settings file path: {settingsFilePath}");

            var appSettingsJson = JsonConvert.SerializeObject(_settings);

            await File.WriteAllTextAsync(settingsFilePath, appSettingsJson);

            Tracer.Debug("Settings saved to disk.");
        }

        public async Task ProcessCreateShortcut(
            string vmId, 
            string name,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            bool? createDesktopShortcut = null,
            bool? createStartMenuShortcut = null,
            CloseAction closeAction = CloseAction.None)
        {
            var appSettings = await Get(true);

            var shortcut = AppSettings.CreateShortcut(name, vmId);

            shortcut.CloseAction = closeAction;
            shortcut.Name = await GetValidShortcutName(shortcut.Id, name);

            appSettings.Shortcuts.Add(shortcut);

            await Save();

            Tracer.Info($"New Shortcut created {shortcut.Id} - \"{shortcut.Name}\" for Virtual Machine {vmId}.");

            if (createDesktopShortcut == true || appSettings.AutoCreateDesktopShortcut)
            {
                Tracer.Info($"Creating desktop shortcut for \"{shortcut.Name}\"...");

                shortcutProvider.CreateDesktopShortcut(shortcut);
            }

            if (createStartMenuShortcut == true || appSettings.AutoCreateStartMenuShortcut)
            {
                Tracer.Info($"Creating start menu shortcut for \"{shortcut.Name}\"...");

                shortcutProvider.CreateStartMenuShortcut(shortcut);
            }

            await trayIpcProvider.SendReloadSettings();
            await trayIpcProvider.SendShowShortcutCreatedNotif(vmId, shortcut.Name);
        }

        public async Task DeleteVirtualMachineShortcuts(
            string vmId,
            ITrayIpcProvider trayIpcProvider)
        {
            var appSettings = await Get(true);

            var matchingShortcuts = appSettings.Shortcuts.Where(x => x.VmId == vmId);

            foreach (var shortcut in matchingShortcuts)
            {
                await trayIpcProvider.SendShowMessageNotif("Shortcut Deleted", $"Shortcut \"{shortcut.Name}\" was automatically deleted.");
            }

            appSettings.Shortcuts.RemoveAll(x => x.VmId == vmId);

            await Save();

            await trayIpcProvider.SendReloadSettings();
        }

        public async Task<bool> ValidateShortcutName(
            string shortcutId,
            string shortcutName)
        {
            var appSettings = await Get();
            return !appSettings.Shortcuts.Any(x => x.Name == shortcutName && x.Id != shortcutId);
        }

        public async Task<string> GetValidShortcutName(
            string shortcutId,
            string vmName)
        {
            var counter = 1;
            var shortcutName = vmName;

            while (!await ValidateShortcutName(shortcutId, shortcutName)
                || counter > 100)
            {
                shortcutName = $"{vmName} ({counter++})";
            }

            return shortcutName;
        }
    }
}

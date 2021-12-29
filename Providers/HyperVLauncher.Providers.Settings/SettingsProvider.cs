using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;
using HyperVLauncher.Providers.Tracing;

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
    }
}

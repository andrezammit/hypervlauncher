using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

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
                var settingsFilePath = _pathProvider.GetSettingsFilePath();

                try
                {
                    var appSettingsJson = await File.ReadAllTextAsync(settingsFilePath);
                    _settings = JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);
                }
                catch
                {
                }

                if (_settings == null)
                {
                    _settings = new AppSettings();
                }
            }

            return _settings;
        }

        public async Task Save()
        {
            if (_settings == null)
            {
                return;
            }

            var settingsFilePath = _pathProvider.GetSettingsFilePath();
            var appSettingsJson = JsonConvert.SerializeObject(_settings);

            await File.WriteAllTextAsync(settingsFilePath, appSettingsJson);
        }
    }
}

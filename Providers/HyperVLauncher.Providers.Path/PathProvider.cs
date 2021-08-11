using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Providers.Path
{
    public class PathProvider : IPathProvider
    {
        private const string _settingsFileName = "settings.json";

        private readonly string _profilePath;

        public PathProvider(string profilePath)
        {
            _profilePath = profilePath;
        }

        public string GetSettingsFilePath()
        {
            return System.IO.Path.Combine(_profilePath, _settingsFileName);
        }
    }
}

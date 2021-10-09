using System;

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

        public static string GetProfileFolder()
        {
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return System.IO.Path.Combine(programDataPath, "HyperVLauncher");
        }
    }
}

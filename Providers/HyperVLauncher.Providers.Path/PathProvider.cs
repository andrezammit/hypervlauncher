using System;
using System.IO;

using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Providers.Path
{
    public class PathProvider : IPathProvider
    {
        public string ProfilePath { get; }

        public PathProvider(string profileName)
        {
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            ProfilePath = System.IO.Path.Combine(programDataPath, profileName);
        }

        public string GetSettingsFilePath()
        {
            return System.IO.Path.Combine(ProfilePath, GeneralConstants.SettingsFileName);
        }

        public string GetTracingPath()
        {
            return System.IO.Path.Combine(ProfilePath, "Logs");
        }

        public void CreateDirectories()
        {
            Directory.CreateDirectory(ProfilePath);

            var tracingPath = GetTracingPath();
            Directory.CreateDirectory(tracingPath);
        }
    }
}

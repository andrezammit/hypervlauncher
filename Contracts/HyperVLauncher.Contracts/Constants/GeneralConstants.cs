
namespace HyperVLauncher.Contracts.Constants
{
    public static class GeneralConstants
    {
        public const string AppName = "Hyper-V Launcher";
        public const string ProfileName = "HyperVLauncher";
        public const string SettingsFileName = "settings.json";

        public const string TrayMutexName = "HyperVLauncher_TrayMutex";
        public const string ConsoleMutexName = "HyperVLauncher_ConsoleMutex";
        public const string LaunchPadMutexName = "HyperVLauncher_LaunchPadMutex";

        public const int TrayIpcPort = 8871;
        public const int MonitorIpcPort = 8870;
        public const int ConsoleIpcPort = 8872;
        public const int LaunchPadIpcPort = 8873;
    }
}

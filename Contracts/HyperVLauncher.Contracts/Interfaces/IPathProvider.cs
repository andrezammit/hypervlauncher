
namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IPathProvider
    {
        string ProfilePath { get; }

        void CreateDirectories();

        string GetTracingPath();
        string GetSettingsFilePath();
    }
}

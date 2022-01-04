using System.Diagnostics;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ISystemFunctionsProvider
    {
        void BringToFront(Process process);

        Process? IsShortcutAlreadyRunning(string shortcutId);
    }
}

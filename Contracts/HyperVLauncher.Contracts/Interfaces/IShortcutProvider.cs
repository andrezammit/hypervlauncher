using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IShortcutProvider
    {
        void CreateDesktopShortcut(Shortcut shortcut);
    }
}

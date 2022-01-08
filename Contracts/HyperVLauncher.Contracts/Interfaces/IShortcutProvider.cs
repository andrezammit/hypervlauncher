using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IShortcutProvider
    {
        void CreateStartupShortcut();
        void CreateDesktopShortcut(Shortcut shortcut);
        void CreateStartMenuShortcut(Shortcut shortcut);

        void DeleteStartupShortcut();
        void DeleteDesktopShortcut(Shortcut shortcut);
        void DeleteStartMenuShortcut(Shortcut shortcut);
    }
}

using System.Threading.Tasks;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ISettingsProvider
    {
        Task Save();

        Task<AppSettings> Get(bool forceReload = false);

        bool ValidateShortcutName(
            string shortcutId,
            string shortcutName,
            AppSettings appSettings);

        string GetValidShortcutName(
            string shortcutId,
            string vmName,
            AppSettings appSettings);

        Task DeleteVirtualMachineShortcuts(
            string vmId,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider);

        Task ProcessCreateShortcut(
            string vmId,
            string vmName,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            bool? createDesktopShortcut = null,
            bool? createStartMenuShortcut = null,
            CloseAction closeAction = CloseAction.None);

        Task ProcessDeleteShortcut(
           Shortcut shortcut,
           ITrayIpcProvider trayIpcProvider,
           IShortcutProvider shortcutProvider);
    }
}

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
            string name,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            bool createDesktopShortcut,
            bool createStartMenuShortcut,
            bool remoteTriggerEnabled = false,
            int listenPort = 0,
            int remotePort = 0,
            CloseAction closeAction = CloseAction.None);

        Task ProcessDeleteShortcut(
           Shortcut shortcut,
           ITrayIpcProvider trayIpcProvider,
           IShortcutProvider shortcutProvider);
    }
}

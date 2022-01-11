using System.Threading.Tasks;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ISettingsProvider
    {
        Task Save();
        Task<AppSettings> Get(bool forceReload = false);

        Task ProcessCreateShortcut(
            string vmId,
            string vmName,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            bool? createDesktopShortcut = null,
            bool? createStartMenuShortcut = null,
            CloseAction closeAction = CloseAction.None);
    }
}

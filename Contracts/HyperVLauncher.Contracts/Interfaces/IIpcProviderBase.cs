using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IIpcProviderBase
    {
        Task SendBringToFront();
        Task SendReloadSettings();

        Task SendMessage(IpcMessage ipcMessage);
        Task SendShowMessageNotif(string title, string message);
        Task SendShowShortcutPromptNotif(string vmId, string vmName);
        Task SendShowShortcutCreatedNotif(string vmId, string shortcutName);

        IAsyncEnumerable<IpcMessage> ReadMessages(CancellationToken cancellationToken);
    }

    public interface ITrayIpcProvider : IIpcProviderBase
    {
    }

    public interface ILaunchPadIpcProvider : IIpcProviderBase
    {
    }

    public interface IIpcProviderAll :
        ITrayIpcProvider,
        ILaunchPadIpcProvider
    {
    }
}
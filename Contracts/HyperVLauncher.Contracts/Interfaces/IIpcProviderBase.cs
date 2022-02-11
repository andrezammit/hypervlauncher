using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IIpcProviderBase
    {
        Task SendReloadSettings();

        IEnumerable<IpcMessage> ReadMessages(
            IList<IpcTopic> topics, 
            CancellationToken cancellationToken);
    }

    public interface ITrayIpcProvider : IIpcProviderBase
    {
        Task SendShowMessageNotif(string title, string message);
        Task SendShowShortcutPromptNotif(string vmId, string vmName);
        Task SendShowShortcutCreatedNotif(string vmId, string shortcutName);
    }

    public interface IConsoleIpcProvider : IIpcProviderBase
    {
    }

    public interface ILaunchPadIpcProvider : IIpcProviderBase
    {
        Task SendBringToFront();
    }

    public interface IMonitorIpcProvider : IIpcProviderBase
    {
        Task RunIpcProxy(CancellationToken cancellationToken);
    }

    public interface IIpcProviderAll :
        ITrayIpcProvider,
        IMonitorIpcProvider,
        ILaunchPadIpcProvider
    {
    }
}
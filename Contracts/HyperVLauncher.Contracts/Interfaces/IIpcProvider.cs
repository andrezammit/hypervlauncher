using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IIpcProvider
    {
        Task SendReloadSettings();
        Task SendMessage(IpcMessage ipcMessage);
        Task SendShowTrayMessage(string title, string message);

        IAsyncEnumerable<IpcMessage> ReadMessages(CancellationToken cancellationToken);
    }
}
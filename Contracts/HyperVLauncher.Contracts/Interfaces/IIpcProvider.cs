using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IIpcProvider
    {
        Task SendMessage(IpcMessage ipcMessage);

        IAsyncEnumerable<IpcMessage> ReadMessages(CancellationToken cancellationToken);
    }
}
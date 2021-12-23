using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Models
{
    public class IpcMessage
    {
        public object? Data { get; init; }

        public IpcCommand IpcCommand { get; init; }
    }
}

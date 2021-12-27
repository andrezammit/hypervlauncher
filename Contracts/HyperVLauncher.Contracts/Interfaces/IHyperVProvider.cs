using System.Diagnostics;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IHyperVProvider
    {
        void StartVirtualMachine(string vmId);
        void PauseVirtualMachine(string vmId);
        void ShutdownVirtualMachine(string vmId);

        string GetVmName(string vmId);

        Process? ConnectVirtualMachine(string vmId);

        IEnumerable<VirtualMachine> GetVirtualMachineList();
    }
}

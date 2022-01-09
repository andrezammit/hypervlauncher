using System;
using System.Diagnostics;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IHyperVProvider
    {
        Func<VirtualMachine, Task>? OnNewVirtualMachine { get; set; }

        void StartVirtualMachine(string vmId);
        void PauseVirtualMachine(string vmId);
        void ShutdownVirtualMachine(string vmId);

        void StartVirtualMachineMonitor(CancellationToken cancellationToken);

        string GetVirtualMachineName(string vmId);

        Process? ConnectVirtualMachine(string vmId);
        VmState GetVirtualMachineState(string vmId);

        IEnumerable<VirtualMachine> GetVirtualMachineList();
    }
}

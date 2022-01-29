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
        Func<VirtualMachine, Task>? OnVirtualMachineCreated { get; set; }
        Func<VirtualMachine, Task>? OnVirtualMachineDeleted { get; set; }

        void StartVirtualMachine(string vmId);
        void PauseVirtualMachine(string vmId);
        void TurnOffVirtualMachine(string vmId);
        void ShutdownVirtualMachine(string vmId);

        void StartVirtualMachineCreatedMonitor(CancellationToken cancellationToken);
        void StartVirtualMachineDeletedMonitor(CancellationToken cancellationToken);

        string GetVirtualMachineName(string vmId);

        Process? ConnectVirtualMachine(string vmId);
        VmState GetVirtualMachineState(string vmId);
        string[]? GetVirtualMachineIpAddresses(string vmId);

        IEnumerable<VirtualMachine> GetVirtualMachineList();
    }
}

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IHyperVProvider
    {
        void StartVirtualMachine(string vmId);
        void ConnectVirtualMachine(string vmName);

        string GetVmName(string vmId);

        IEnumerable<VirtualMachine> GetVirtualMachineList();
    }
}

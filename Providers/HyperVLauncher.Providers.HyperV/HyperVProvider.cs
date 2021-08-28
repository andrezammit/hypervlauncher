using System.Management;
using System.Diagnostics;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Providers.HyperV
{
    public class HyperVProvider : IHyperVProvider
    {
        public IEnumerable<VirtualMachine> GetVirtualMachineList()
        {
            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem");

            using var searcher = new ManagementObjectSearcher(scope, query);

            foreach (var queryObj in searcher.Get())
            {
                var vmId = queryObj["Name"].ToString();
                var vmName = queryObj["ElementName"].ToString();

                if (!string.IsNullOrEmpty(vmId) &&
                    !string.IsNullOrEmpty(vmName))
                {
                    yield return new VirtualMachine(vmId, vmName);
                }
            }
        }

        public void StartVirtualMachine(string vmId)
        {
            var vmObject = GetVmObject(vmId);

            if (vmObject == null)
            {
                return;
            }

            var inParams = vmObject.GetMethodParameters("RequestStateChange");

            inParams["RequestedState"] = 2;

            var outParams = vmObject.InvokeMethod(
                "RequestStateChange",
                inParams,
                null);
        }

        public string GetVmName(string vmId)
        {
            var vmObject = GetVmObject(vmId);

            if (vmObject == null)
            {
                throw new InvalidOperationException($"Virtual machine {vmId} was not found.");
            }

            var vmName = vmObject["ElementName"].ToString();

            if (string.IsNullOrEmpty(vmName))
            {
                throw new InvalidOperationException($"Could not find name for virtual machine {vmId}.");
            }

            return vmName;
        }

        private static ManagementObject? GetVmObject(string vmId)
        {
            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new SelectQuery($"SELECT * FROM Msvm_ComputerSystem WHERE Name = \"{vmId}\"");

            using var searcher = new ManagementObjectSearcher(scope, query);
            using var searchResults = searcher.Get();

            foreach (ManagementObject vmObject in searchResults)
            {
                return vmObject;
            }

            return null;
        }

        public void ConnectVirtualMachine(string vmId)
        {
            var startInfo = new ProcessStartInfo("vmconnect.exe", $"{Environment.MachineName} -G \"{vmId}\"");

            using (Process.Start(startInfo))
            {

            }
        }
    }
}

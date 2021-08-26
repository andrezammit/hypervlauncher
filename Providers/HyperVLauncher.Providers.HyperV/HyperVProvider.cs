using System.Management;
using System.Diagnostics;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Providers.HyperV
{
    public class HyperVProvider
    {
        public static IEnumerable<VirtualMachine> GetVirtualMachineList()
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

        public static void StartVirtualMachine(string vmId)
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

        public static string GetVmName(string vmId)
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

        public static void ConnectVirtualMachine(string vmName)
        {
            var startInfo = new ProcessStartInfo("vmconnect.exe", $"localhost \"{vmName}\"");

            using (Process.Start(startInfo))
            {

            }
        }
    }
}

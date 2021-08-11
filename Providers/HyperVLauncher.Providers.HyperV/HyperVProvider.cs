using System.Management;
using System.Diagnostics;
using System.Collections.Generic;

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
                var vmName = queryObj["ElementName"].ToString();

                if (!string.IsNullOrEmpty(vmName))
                {
                    yield return new VirtualMachine(vmName);
                }
            }
        }

        public static void StartVirtualMachine(string vmName)
        {
            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new SelectQuery($"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = \"{vmName}\"");

            using var searcher = new ManagementObjectSearcher(scope, query);
            using var searchResults = searcher.Get();

            foreach (ManagementObject vmObject in searchResults)
            {
                var inParams = vmObject.GetMethodParameters("RequestStateChange");

                inParams["RequestedState"] = 2;

                var outParams = vmObject.InvokeMethod(
                    "RequestStateChange",
                    inParams,
                    null);
            }
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

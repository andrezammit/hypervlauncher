using System.Management;
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
    }
}

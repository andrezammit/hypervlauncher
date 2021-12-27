using System;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;

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

            _ = vmObject.InvokeMethod(
                "RequestStateChange",
                inParams,
                null);
        }

        public void PauseVirtualMachine(string vmId)
        {
            var vmObject = GetVmObject(vmId);

            if (vmObject == null)
            {
                return;
            }

            var inParams = vmObject.GetMethodParameters("RequestStateChange");

            inParams["RequestedState"] = 6;

            _ = vmObject.InvokeMethod(
                "RequestStateChange",
                inParams,
                null);
        }

        public void ShutdownVirtualMachine(string vmId)
        {
            var vmObject = GetVmObject(vmId);

            if (vmObject is null)
            {
                return;
            }

            var relPath = vmObject.GetPropertyValue("__RELPATH").ToString();

            if (relPath is null)
            {
                return;
            }

            var shutdownComponent = GetVmShutdownComponent(relPath);

            if (shutdownComponent is null)
            {
                return;
            }

            var inParams = shutdownComponent.GetMethodParameters("InitiateShutdown");

            inParams["Force"] = true;
            inParams["Reason"] = "Hyper-V Launcher shutdown.";

            _ = shutdownComponent.InvokeMethod(
                "InitiateShutdown",
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
            using var searcher = new ManagementObjectSearcher(
                "\\\\.\\root\\virtualization\\v2", 
                $"SELECT * FROM Msvm_ComputerSystem WHERE Name = \"{vmId}\"");

            using var searchResults = searcher.Get();

            foreach (ManagementObject vmObject in searchResults)
            {
                return vmObject;
            }

            return null;
        }

        private static ManagementObject? GetVmShutdownComponent(string relPath)
        {
            using var searcher = new ManagementObjectSearcher(
                "\\\\.\\root\\virtualization\\v2", 
                $"Associators of {{{relPath}}} where AssocClass=Msvm_SystemDevice ResultClass=Msvm_ShutdownComponent");

            using var searchResults = searcher.Get();

            foreach (ManagementObject shutdownComponent in searchResults)
            {
                return shutdownComponent;
            }

            return null;
        }

        public Process? ConnectVirtualMachine(string vmId)
        {
            var startInfo = new ProcessStartInfo("vmconnect.exe", $"{Environment.MachineName} -G \"{vmId}\"");

            return Process.Start(startInfo);
        }
    }
}

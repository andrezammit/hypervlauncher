using System;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;
using HyperVLauncher.Providers.HyperV.Contracts.Enums;

namespace HyperVLauncher.Providers.HyperV
{
    public class HyperVProvider : IHyperVProvider
    {
        private const string _virtualizationScope = "\\\\.\\root\\virtualization\\v2";

        public IEnumerable<VirtualMachine> GetVirtualMachineList()
        {
            using var searcher = new ManagementObjectSearcher(
                _virtualizationScope, 
                "SELECT * FROM Msvm_ComputerSystem");

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

            WaitForJobToFinish(outParams);
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

            var outParams = vmObject.InvokeMethod(
                "RequestStateChange",
                inParams,
                null);

            WaitForJobToFinish(outParams);
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

            var outParams = shutdownComponent.InvokeMethod(
                "InitiateShutdown",
                inParams,
                null);

            WaitForJobToFinish(outParams);
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
                _virtualizationScope, 
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
                _virtualizationScope, 
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

        private static void WaitForJobToFinish(ManagementBaseObject outParams)
        {
            var returnValue = (UInt32)outParams["ReturnValue"];

            switch (returnValue)
            {
                case WmiReturnCode.Started:
                    WaitForStartedJobToFinish(outParams);
                    break;

                case WmiReturnCode.Completed:
                    return;

                default:
                    throw new InvalidOperationException($"WMI operation failed with return value {returnValue}.");
            }
        }

        private static void WaitForStartedJobToFinish(ManagementBaseObject outParams)
        {
            var jobPath = (string)outParams["Job"];
            var job = new ManagementObject(_virtualizationScope, jobPath, null);

            job.Get();

            while ((UInt16)job["JobState"] == WmiJobState.Starting
                || (UInt16)job["JobState"] == WmiJobState.Running)
            {
                Thread.Sleep(1000);

                job.Get();
            }

            var finalJobState = (UInt16)job["JobState"];

            if (finalJobState != WmiJobState.Completed)
            {
                var jobErrorCode = (UInt16)job["ErrorCode"];
                var jobErrorDescription = (string)job["ErrorDescription"];

                throw new InvalidOperationException($"WMI operation failed. Error code: {jobErrorCode}, Error description: {jobErrorDescription}");
            }
        }
    }
}

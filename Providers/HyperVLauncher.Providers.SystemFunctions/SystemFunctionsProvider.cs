using System;
using System.Linq;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Providers.SystemFunctions
{
    public class SystemFunctionsProvider : ISystemFunctionsProvider
    {
        private static IEnumerable<Process> GetRunningProcesses()
        {
            foreach (var process in Process.GetProcesses())
            {
                yield return process;
            }
        }

        public Process? IsShortcutAlreadyRunning(string shortcutId)
        {
            var launchPadProcesses = GetRunningProcesses()
                .Where(x => x.ProcessName == "HyperVLauncher.Apps.LaunchPad");

            foreach (var launchPadProcess in launchPadProcesses)
            {
                var commandLine = GetCommandLine(launchPadProcess);

                if (string.IsNullOrEmpty(commandLine))
                {
                    continue;
                }

                if (commandLine.Contains(shortcutId))
                {
                    return launchPadProcess;
                }
            }

            return null;
        }

        private static string? GetCommandLine(Process process)
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");

            using var objects = searcher.Get();

            var wmiProcess = objects
                .Cast<ManagementBaseObject>()
                .SingleOrDefault();

            if (wmiProcess is null)
            {
                return null;
            }

            return wmiProcess["CommandLine"]?.ToString();
        }

        public void BringToFront(Process process)
        {
            var hwnd = process.MainWindowHandle;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);


        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        private const int SW_RESTORE = 9;
    }
}
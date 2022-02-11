using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.Common
{
    public static class GenericHelpers
    {
        public static Mutex? TakeInstanceMutex(string mutexName)
        {
            var instanceMutex = new Mutex(true, mutexName, out var createdNew);

            if (!createdNew)
            {
                instanceMutex.Dispose();
                return null;
            }

            return instanceMutex;
        }

        public static bool IsMutexAvailable(string mutexName)
        {
            try
            {
                using var mutex = Mutex.OpenExisting(mutexName);
                
                return false;
            }
            catch
            {
            }

            return true;
        }

        public static void LaunchShortcut(
            string shortcutId,
            ILaunchPadIpcProvider launchPadIpcProvider)
        {
            try
            {
                var launchPadMutexName = $"{GeneralConstants.LaunchPadMutexName}_{shortcutId}";

                if (!IsMutexAvailable(launchPadMutexName))
                {
                    Tracer.Info($"Shortcut {shortcutId} is already running.");

                    launchPadIpcProvider.SendBringToFront();

                    return;
                }

                Tracer.Info($"Launching shortcut {shortcutId}...");

                var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.LaunchPad.exe", shortcutId)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                };

                using (Process.Start(startInfo))
                {
                }
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to launch shortcut {shortcutId}.", ex);
            }
        }

        public static void LaunchConsole()
        {
            try
            {
                var runningInstances = Process.GetProcessesByName("HyperVLauncher.Apps.Console");

                if (runningInstances.FirstOrDefault() is Process runningInstance)
                {
                    Tracer.Debug($"Console already running.");

                    BringToFront(runningInstance);
                    return;
                }

                Tracer.Debug($"Launching Console...");

                var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.Console.exe")
                {
                    Verb = "runas",
                    UseShellExecute = true
                };

                using (Process.Start(startInfo))
                {
                }
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to launch Console.", ex);
            }
        }

        public static void LaunchHyperVManager()
        {
            try
            {
                Tracer.Debug($"Launching Hyper-V Manager...");

                var startInfo = new ProcessStartInfo("Virtmgmt.msc")
                {
                    Verb = "runas",
                    UseShellExecute = true
                };

                using (Process.Start(startInfo))
                {
                }
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to launch Hyper-V Manager.", ex);
            }
        }

        public static async Task HandleShortcutExitBehaviour(
            IHyperVProvider hyperVProvider,
            ITrayIpcProvider trayIpcProvider,
            Shortcut shortcut)
        {
            switch (shortcut.CloseAction)
            {
                case HyperVLauncher.Contracts.Enums.CloseAction.Pause:
                    Tracer.Info($"Pausing {shortcut.Name}...");

                    await trayIpcProvider.SendShowMessageNotif("Virtual Machine State Change", $"Pausing {shortcut.Name}...");
                    hyperVProvider.PauseVirtualMachine(shortcut.VmId);

                    Tracer.Info($"{shortcut.Name} paused.");

                    break;

                case HyperVLauncher.Contracts.Enums.CloseAction.Shutdown:
                    Tracer.Info($"Shutting down {shortcut.Name}...");

                    await trayIpcProvider.SendShowMessageNotif("Virtual Machine State Change", $"Shutting down {shortcut.Name}...");
                    hyperVProvider.ShutdownVirtualMachine(shortcut.VmId);

                    Tracer.Info($"{shortcut.Name} shut down.");

                    break;

                default:
                    break;
            }
        }

        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();

            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();

            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();

            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
            {
                if (!portArray.Contains(i))
                {
                    return i;
                }
            }

            return 0;
        }

        public static void BringToFront(this Process process)
        {
            var hwnd = process.MainWindowHandle;

            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }

        public static void ShowConsoleWindow()
        {
            AllocConsole();
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);


        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        private const int SW_RESTORE = 9;
    }
}

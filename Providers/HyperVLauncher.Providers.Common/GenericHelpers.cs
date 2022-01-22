using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;
using System.Linq;

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

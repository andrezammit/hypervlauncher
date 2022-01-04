using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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

        public static void BringToFront(this Process process)
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

using System;
using System.Text;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using HyperVLauncher.Contracts.Interfaces;

using IoPath = System.IO.Path;
using ShortcutModel = HyperVLauncher.Contracts.Models.Shortcut;

namespace HyperVLauncher.Providers.Shortcut
{
    public class ShortcutProvider : IShortcutProvider
    {
        public void CreateDesktopShortcut(ShortcutModel shortcut)
        {
            var shellLink = (IShellLink)new ShellLink();

            shellLink.SetArguments(shortcut.Id);
            shellLink.SetDescription($"Launch Hyper-V shortcut {shortcut.Name}.");
            shellLink.SetIconLocation($"{AppContext.BaseDirectory}\\Icons\\shortcut.ico", 0);
            shellLink.SetPath($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.LaunchPad.exe");

            var shortcutFile = (IPersistFile)shellLink;
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = IoPath.Combine(desktopPath, $"{shortcut.Name}.lnk");

            shortcutFile.Save(shortcutPath, false);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Hardcodet.Wpf.TaskbarNotification;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Settings;

namespace HyperVLauncher.Apps.Tray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Task? _ipcProcessor;

        private readonly TaskbarIcon _taskbarIcon = new();
        private readonly IpcProvider _ipcProvider = new("HyperVLauncherIpc");
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (!GenericHelpers.IsUniqueInstance("HyperVLauncherTrayMutex"))
            {
                base.Shutdown();

                return;
            }

            _ipcProcessor = Task.Run(
                () => ProcessIpcMessages(_cancellationTokenSource.Token));

            _taskbarIcon.ContextMenu = new ContextMenu();
            _taskbarIcon.ToolTipText = "Hyper-V Launcher";
            _taskbarIcon.Icon = new System.Drawing.Icon("Icons\\app.ico");
            _taskbarIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;

            _taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;

            CreateContextMenu();
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _taskbarIcon.ContextMenu.Items.Clear();

            var titleMenuItem = new MenuItem()
            {
                Header = "Hyper-V Launcher Console"
            };

            titleMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                LaunchConsole();
            };

            _taskbarIcon.ContextMenu.Items.Add(titleMenuItem);
            _taskbarIcon.ContextMenu.Items.Add(new Separator());

            var profilePath = PathProvider.GetProfileFolder();
            Directory.CreateDirectory(profilePath);

            var pathProvider = new PathProvider(profilePath);
            var settingsProvider = new SettingsProvider(pathProvider);

            var appSettings = settingsProvider
                .Get(true)
                .GetAwaiter()
                .GetResult();

            foreach (var shortcut in appSettings.Shortcuts)
            {
                var menuItem = new MenuItem()
                {
                    Header = shortcut.Name
                };

                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    LaunchShortcut(shortcut.Id);
                };

                _taskbarIcon.ContextMenu.Items.Add(menuItem);
            }

            var closeMenuItem = new MenuItem()
            {
                Header = "Close"
            };

            closeMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                base.Shutdown();
            };

            _taskbarIcon.ContextMenu.Items.Add(new Separator());
            _taskbarIcon.ContextMenu.Items.Add(closeMenuItem);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _ipcProcessor?
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // Swallow any exception since we're closing anyway.
            }

            base.OnExit(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            LaunchConsole();
        }

        private static void LaunchShortcut(string shortcutId)
        {
            var startInfo = new ProcessStartInfo("HyperVLauncher.Apps.LaunchPad.exe", shortcutId);

            using (Process.Start(startInfo))
            {
            }
        }

        private static void LaunchConsole()
        {
            var startInfo = new ProcessStartInfo("HyperVLauncher.Apps.Console.exe");

            using (Process.Start(startInfo))
            {
            }
        }

        private async Task ProcessIpcMessages(CancellationToken cancellationToken)
        {
            await foreach (var ipcMessage in _ipcProvider.ReadMessages(cancellationToken).ConfigureAwait(true))
            {
                switch (ipcMessage.IpcCommand)
                {
                    case Contracts.Enums.IpcCommand.ReloadSettings:
                        CreateContextMenu();
                        break;

                    default:
                        throw new InvalidDataException($"Invalid IPC command: {ipcMessage.IpcCommand}");
                }
            }
        }
    }
}

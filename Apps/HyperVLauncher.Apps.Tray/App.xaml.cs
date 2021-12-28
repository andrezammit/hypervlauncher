using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using Hardcodet.Wpf.TaskbarNotification;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Settings;
using Newtonsoft.Json.Linq;

namespace HyperVLauncher.Apps.Tray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Task? _ipcProcessor;

        private readonly TaskbarIcon _taskbarIcon = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly IpcProvider _ipcProvider = new(GeneralConstants.IpcPipeName);

        private readonly MenuItem _titleMenuItem;
        private readonly MenuItem _closeMenuItem;

        public App()
        {
            _titleMenuItem = new MenuItem()
            {
                Header = "Hyper-V Launcher Console"
            };

            _titleMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                LaunchConsole();
            };

            _closeMenuItem = new MenuItem()
            {
                Header = "Close"
            };

            _closeMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                base.Shutdown();
            };
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (!GenericHelpers.IsUniqueInstance(GeneralConstants.TrayMutexName))
            {
                base.Shutdown();

                return;
            }

            _ipcProcessor = Task.Run(
                () => ProcessIpcMessages(_cancellationTokenSource.Token));

            _taskbarIcon.ContextMenu = new ContextMenu();
            _taskbarIcon.ToolTipText = "Hyper-V Launcher";
            _taskbarIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;
            _taskbarIcon.Icon = new System.Drawing.Icon($"{AppContext.BaseDirectory}\\Icons\\app.ico");

            _taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _taskbarIcon.ContextMenu.Items.Clear();

            _taskbarIcon.ContextMenu.Items.Add(_titleMenuItem);
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

            if (appSettings.Shortcuts.Count > 0)
            {
                _taskbarIcon.ContextMenu.Items.Add(new Separator());
            }

            _taskbarIcon.ContextMenu.Items.Add(_closeMenuItem);
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
            var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.LaunchPad.exe", shortcutId)
            {
                Verb = "runas",
                UseShellExecute = true
            };

            using (Process.Start(startInfo))
            {
            }
        }

        private static void LaunchConsole()
        {
            var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.Console.exe")
            {
                Verb = "runas",
                UseShellExecute = true
            };

            using (Process.Start(startInfo))
            {
            }
        }

        private void ShowMessage(JObject? ipcMessageData)
        {
            if (ipcMessageData is null)
            {
                return;
            }

            var trayMessageData = ipcMessageData.ToObject<TrayMessageData>();

            if (trayMessageData is null)
            {
                return;
            }

            _taskbarIcon.ShowBalloonTip(
                trayMessageData.Title, 
                trayMessageData.Message, 
                BalloonIcon.Info);
        }

        private async Task ProcessIpcMessages(CancellationToken cancellationToken)
        {
            await foreach (var ipcMessage in _ipcProvider.ReadMessages(cancellationToken))
            {
                switch (ipcMessage.IpcCommand)
                {
                    case IpcCommand.ReloadSettings:
                        Current.Dispatcher.Invoke(new Action(() => CreateContextMenu()));
                        break;

                    case IpcCommand.ShowTrayMessage:
                        Current.Dispatcher.Invoke(new Action(() => ShowMessage(ipcMessage.Data as JObject)));
                        break;

                    default:
                        throw new InvalidDataException($"Invalid IPC command: {ipcMessage.IpcCommand}");
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json.Linq;

using Hardcodet.Wpf.TaskbarNotification;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Settings;
using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Apps.Tray
{
    public partial class App : Application
    {
        private Task? _ipcProcessor;

        private readonly TaskbarIcon _taskbarIcon = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly IIpcProvider _ipcProvider = new IpcProvider(GeneralConstants.IpcPipeName);
        private readonly IPathProvider _pathProvider = new PathProvider(GeneralConstants.ProfileName);

        private readonly MenuItem _titleMenuItem;
        private readonly MenuItem _closeMenuItem;

        public App()
        {
            _pathProvider.CreateDirectories();

            TracingProvider.Init(_pathProvider.GetTracingPath(), "Tray");

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
            try
            {
                Tracer.Debug("Creating context menu...");

                _taskbarIcon.ContextMenu.Items.Clear();

                _taskbarIcon.ContextMenu.Items.Add(_titleMenuItem);
                _taskbarIcon.ContextMenu.Items.Add(new Separator());

                var settingsProvider = new SettingsProvider(_pathProvider);

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
            catch (Exception ex)
            {
                Tracer.Error("Failed to create context menu.", ex);
            }
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
            catch (Exception ex)
            {
                Tracer.Error("Failed to close IPC processor.", ex);

                // Swallow any exception since we're closing anyway.
            }

            Tracer.Debug("Closing Tray app...");

            base.OnExit(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            LaunchConsole();
        }

        private static void LaunchShortcut(string shortcutId)
        {
            try
            {
                Tracer.Info($"Launching shortcut {shortcutId}...");

                var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.LaunchPad.exe", shortcutId)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

#if DEBUG
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
#endif

                using (Process.Start(startInfo))
                {
                }
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to launch shortcut {shortcutId}.", ex);
            }
        }

        private static void LaunchConsole()
        {
            try
            {
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

        private void ShowMessage(JObject? ipcMessageData)
        {
            try
            {
                Tracer.Debug("Balloon message notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData JSON object received.");

                    return;
                }

                var trayMessageData = ipcMessageData.ToObject<TrayMessageData>();

                if (trayMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData data received.");

                    return;
                }

                Tracer.Debug($"Showing balloon message: {trayMessageData.Title} - {trayMessageData.Message}");

                _taskbarIcon.ShowBalloonTip(
                    trayMessageData.Title,
                    trayMessageData.Message,
                    BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                Tracer.Warning($"Failed to show balloon with message.", ex);
            }
        }

        private async Task ProcessIpcMessages(CancellationToken cancellationToken)
        {
            try
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
            catch (OperationCanceledException)
            {
                // Swallow.
            }
            catch (Exception ex)
            {
                Tracer.Error("Error while processing IPC messages.", ex);

                throw;
            }
        }
    }
}

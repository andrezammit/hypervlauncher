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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;

namespace HyperVLauncher.Apps.Tray
{
    public partial class App : Application
    {
        private Task? _ipcProcessor;
        private Mutex? _instanceMutex;

        private readonly TaskbarIcon _taskbarIcon = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly IPathProvider _pathProvider = new PathProvider(GeneralConstants.ProfileName);
        private readonly ITrayIpcProvider _trayIpcProvider = new IpcProvider(GeneralConstants.TrayIpcPipeName);
        private readonly ILaunchPadIpcProvider _launchPadIpcProvider = new IpcProvider(GeneralConstants.LaunchPadIpcPipeName);

        private readonly MenuItem _titleMenuItem;
        private readonly MenuItem _closeMenuItem;

        private readonly Uri _notifIconUri = new($"file:///{Path.GetFullPath("Icons/shortcut.png")}");

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
                GenericHelpers.LaunchConsole();
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
            _instanceMutex = GenericHelpers.TakeInstanceMutex(GeneralConstants.TrayMutexName);

            if (_instanceMutex is null)
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

            _taskbarIcon.TrayBalloonTipShown += TaskbarIcon_TrayBalloonTipShown;
            _taskbarIcon.TrayBalloonTipClicked += TaskbarIcon_TrayBalloonTipClicked;

            CreateContextMenu();

            //Task.Run(async () =>
            //{
            //    await Task.Delay(2000);

            //    Current.Dispatcher.Invoke(new Action(() =>
            //    {
            //        new ToastContentBuilder()
            //            .AddAppLogoOverride(_notifIconUri)
            //            .AddArgument("action", "viewConversation")
            //            .AddArgument("conversationId", 9813)
            //            .AddText("New Virtual Machine Detected", AdaptiveTextStyle.Header)
            //            .AddText("Create a new shortcut for \"Windows 10\"?")
            //            .AddButton(new ToastButton()
            //                .SetContent("Yes")
            //                .AddArgument("action", "create"))
            //            .AddButton(new ToastButtonDismiss("No"))
            //            .Show();
            //    }));
            //});

            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ShowMessageNotif(string title, string message)
        {
            Current.Dispatcher.Invoke(delegate
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(_notifIconUri)
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText(title, AdaptiveTextStyle.Header)
                    .AddText(message)
                    .Show();
            });
        }

        private void ShowShortcutCreatedNotif(string vmName)
        {
            Current.Dispatcher.Invoke(delegate
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(_notifIconUri)
                    .AddArgument("action", "openConsole")
                    .AddText("New Virtual Machine Detected", AdaptiveTextStyle.Header)
                    .AddText($"A new Virtual Machine shortcut was created for \"{vmName}\".")
                    .Show();
            });
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var toastArgs = ToastArguments.Parse(e.Argument);

            if (toastArgs["action"] == "openConsole")
            {
                GenericHelpers.LaunchConsole();
                return;
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                // TODO: Show the corresponding content
                MessageBox.Show("Toast activated. Args: " + toastArgs);
            });
        }

        internal enum BalloonAction
        {
            None,
            Console,
            AddShortcut
        }

        internal class BalloonEvent
        {
            public object? Sender { get; set; }
            public BalloonAction BalloonAction { get; set; }
        }

        internal List<BalloonEvent> _waitingEvents = new();

        private void TaskbarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            var balloonEvent = _waitingEvents.FirstOrDefault(x => x.Sender == sender);

            if (balloonEvent is not null)
            {
                MessageBox.Show(balloonEvent.BalloonAction.ToString());
            }
        }

        private void TaskbarIcon_TrayBalloonTipShown(object sender, RoutedEventArgs e)
        {
            var balloonEvent = _waitingEvents.FirstOrDefault(x => x.Sender is null);

            if (balloonEvent is not null)
            {
                balloonEvent.Sender = sender;
            }
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
                        GenericHelpers.LaunchShortcut(
                            shortcut.Id,
                            _launchPadIpcProvider);
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

            _instanceMutex?.Dispose();

            Tracer.Debug("Closing Tray app...");

            base.OnExit(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            GenericHelpers.LaunchConsole();
        }

        private void ShowMessageNotif(JObject? ipcMessageData)
        {
            try
            {
                Tracer.Debug("Balloon message notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData JSON object received.");

                    return;
                }

                var trayMessageData = ipcMessageData.ToObject<ShowMessageNotifData>();

                if (trayMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData data received.");

                    return;
                }

                Tracer.Debug($"Showing balloon message: {trayMessageData.Title} - {trayMessageData.Message}");

                ShowMessageNotif(trayMessageData.Title, trayMessageData.Message);
            }
            catch (Exception ex)
            {
                Tracer.Warning($"Failed to show balloon with message.", ex);
            }
        }

        private void ShowShortcutCreatedNotif(JObject? ipcMessageData)
        {
            try
            {
                Tracer.Debug("Balloon message notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData JSON object received.");

                    return;
                }

                var shortcutCreatedNotifData = ipcMessageData.ToObject<ShortcutCreatedNotifData>();

                if (shortcutCreatedNotifData is null)
                {
                    Tracer.Debug("Invalid ShortcutCreatedNotifData data received.");

                    return;
                }

                Tracer.Debug($"Showing shortcut created notification: {shortcutCreatedNotifData.VmId} - {shortcutCreatedNotifData.VmName}");

                ShowShortcutCreatedNotif(shortcutCreatedNotifData.VmName);
            }
            catch (Exception ex)
            {
                Tracer.Warning($"Failed to show shortcut created notification.", ex);
            }
        }

        private async Task ProcessIpcMessages(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var ipcMessage in _trayIpcProvider.ReadMessages(cancellationToken))
                {
                    switch (ipcMessage.IpcCommand)
                    {
                        case IpcCommand.ReloadSettings:
                            Current.Dispatcher.Invoke(new Action(() => CreateContextMenu()));
                            break;

                        case IpcCommand.ShowMessageNotif:
                            ShowMessageNotif(ipcMessage.Data as JObject);
                            break;

                        case IpcCommand.ShowShortcutCreatedNotif:
                            ShowShortcutCreatedNotif(ipcMessage.Data as JObject);
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

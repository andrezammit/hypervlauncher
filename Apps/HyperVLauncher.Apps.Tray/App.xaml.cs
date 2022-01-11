using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Microsoft.Toolkit.Uwp.Notifications;

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
using HyperVLauncher.Providers.Shortcut;

namespace HyperVLauncher.Apps.Tray
{
    public partial class App : Application
    {
        private Task? _ipcProcessor;
        private Mutex? _instanceMutex;

        private readonly TaskbarIcon _taskbarIcon = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly ISettingsProvider _settingsProvider;

        private readonly IShortcutProvider _shortcutProvider = new ShortcutProvider();
        private readonly IPathProvider _pathProvider = new PathProvider(GeneralConstants.ProfileName);
        private readonly ITrayIpcProvider _trayIpcProvider = new IpcProvider(GeneralConstants.TrayIpcPipeName);
        private readonly ILaunchPadIpcProvider _launchPadIpcProvider = new IpcProvider(GeneralConstants.LaunchPadIpcPipeName);

        private readonly MenuItem _titleMenuItem;
        private readonly MenuItem _closeMenuItem;

        private readonly Uri _notifVmIconUri = new($"file:///{Path.GetFullPath("Icons/vm.png")}");
        private readonly Uri _notifShortcutIconUri = new($"file:///{Path.GetFullPath("Icons/shortcut.png")}");

        public App()
        {
            _settingsProvider = new SettingsProvider(_pathProvider);

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

            CreateContextMenu();

            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ShowMessageNotif(string title, string message)
        {
            Current.Dispatcher.Invoke(delegate
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(_notifShortcutIconUri)
                    .AddArgument("action", "openConsole")
                    .AddText(title, AdaptiveTextStyle.Header)
                    .AddText(message)
                    .Show();
            });
        }

        private void ShowShortcutCreatedNotif(string shortcutName)
        {
            Current.Dispatcher.Invoke(delegate
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(_notifShortcutIconUri)
                    .AddArgument("action", "openConsole")
                    .AddText("Virtual Machine Shortcut Created", AdaptiveTextStyle.Header)
                    .AddText($"A new Virtual Machine shortcut \"{shortcutName}\" was created.")
                    .Show();
            });
        }

        private void ShowShortcutPromptNotif(string vmId, string vmName)
        {
            Current.Dispatcher.Invoke(delegate
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(_notifVmIconUri)
                    .AddArgument("action", "openConsole")
                    .AddText("New Virtual Machine Detected", AdaptiveTextStyle.Header)
                    .AddText($"Create a new Virtual Machine shortcut for \"{vmName}\"?")
                    .AddButton(new ToastButton()
                        .SetContent("Yes")
                        .AddArgument("action", "createShortcut")
                        .AddArgument("vmId", vmId)
                        .AddArgument("vmName", vmName))
                    .AddButton(new ToastButtonDismiss("No"))
                    .Show();
            });
        }

        private async void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var toastArgs = ToastArguments.Parse(e.Argument);

            switch (toastArgs["action"])
            {
                case "openConsole":
                    GenericHelpers.LaunchConsole();
                    break;

                case "createShortcut":
                    {
                        var vmId = toastArgs["vmId"];
                        var vmName = toastArgs["vmName"];

                        await _settingsProvider.ProcessCreateShortcut(
                            vmId,
                            vmName,
                            _trayIpcProvider,
                            _shortcutProvider);
                    }

                    break;

                default:
                    throw new NotSupportedException($"Invalid notification arguments: {e.Argument}");
            }

            if (toastArgs["action"] == "openConsole")
            {
                GenericHelpers.LaunchConsole();
                return;
            }
        }

        internal enum BalloonAction
        {
            None,
            Console,
            AddShortcut
        }

        private void CreateContextMenu()
        {
            try
            {
                Tracer.Debug("Creating context menu...");

                _taskbarIcon.ContextMenu.Items.Clear();

                _taskbarIcon.ContextMenu.Items.Add(_titleMenuItem);
                _taskbarIcon.ContextMenu.Items.Add(new Separator());


                var appSettings = _settingsProvider
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
                Tracer.Debug("Show message notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid TrayMessageData JSON object received.");

                    return;
                }

                var trayMessageData = ipcMessageData.ToObject<ShowMessageNotifData>();

                if (trayMessageData is null)
                {
                    Tracer.Debug("Invalid ShowMessageNotifData data received.");

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
                Tracer.Debug("Show shortcut created notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid ShortcutCreatedNotifData JSON object received.");

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

        private void ShowShortcutPromptNotif(JObject? ipcMessageData)
        {
            try
            {
                Tracer.Debug("Show shortcut prompt notification received.");

                if (ipcMessageData is null)
                {
                    Tracer.Debug("Invalid ShortcutPromptNotifData JSON object received.");

                    return;
                }

                var shortcutPromptNotifData = ipcMessageData.ToObject<ShortcutPromptNotifData>();

                if (shortcutPromptNotifData is null)
                {
                    Tracer.Debug("Invalid ShortcutPromptNotifData data received.");

                    return;
                }

                Tracer.Debug($"Showing shortcut prompt notification: {shortcutPromptNotifData.VmId} - {shortcutPromptNotifData.VmName}");

                ShowShortcutPromptNotif(shortcutPromptNotifData.VmId, shortcutPromptNotifData.VmName);
            }
            catch (Exception ex)
            {
                Tracer.Warning($"Failed to show shortcut prompt notification.", ex);
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

                        case IpcCommand.ShowShortcutPromptNotif:
                            ShowShortcutPromptNotif(ipcMessage.Data as JObject);
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

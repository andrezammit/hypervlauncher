using System.IO;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;

using Hardcodet.Wpf.TaskbarNotification;

using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Settings;

namespace HyperVLauncher.Apps.Tray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly TaskbarIcon taskbarIcon = new();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            taskbarIcon.ContextMenu = new ContextMenu();
            taskbarIcon.ToolTipText = "Hyper-V Launcher";
            taskbarIcon.Icon = new System.Drawing.Icon("Icons\\app.ico");
            taskbarIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;

            taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;

            var titleMenuItem = new MenuItem()
            {
                Header = "Hyper-V Launcher Console"
            };

            titleMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                LaunchConsole();
            };

            taskbarIcon.ContextMenu.Items.Add(titleMenuItem);
            taskbarIcon.ContextMenu.Items.Add(new Separator());

            var profilePath = PathProvider.GetProfileFolder();
            Directory.CreateDirectory(profilePath);

            var pathProvider = new PathProvider(profilePath);
            var settingsProvider = new SettingsProvider(pathProvider);

            var appSettings = settingsProvider
                .Get()
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

                taskbarIcon.ContextMenu.Items.Add(menuItem);
            }

            var closeMenuItem = new MenuItem()
            {
                Header = "Close"
            };

            closeMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                base.Shutdown();
            };

            taskbarIcon.ContextMenu.Items.Add(new Separator());
            taskbarIcon.ContextMenu.Items.Add(closeMenuItem);
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
    }
}

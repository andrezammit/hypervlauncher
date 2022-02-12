using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Pages
{
    public partial class SettingsPage : Page
    {
        private bool _isLoading;

        private readonly ITrayIpcProvider _trayIpcProvider;
        private readonly IShortcutProvider _shortcutProvider;
        private readonly ISettingsProvider _settingsProvider;

        public SettingsPage(
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _trayIpcProvider = trayIpcProvider;
            _shortcutProvider = shortcutProvider;
            _settingsProvider = settingsProvider;
        }

        private void EnableControls()
        {
            chkNotifyOnNewVm.IsEnabled = !chkAutoCreateShortcuts.IsChecked.GetValueOrDefault();

            var autoCreateVms =
                chkAutoCreateShortcuts.IsChecked.GetValueOrDefault() ||
                chkNotifyOnNewVm.IsChecked.GetValueOrDefault();

            lblAutoCreatedShortcuts.IsEnabled = autoCreateVms;
            chkAutoCreateDesktopShortcut.IsEnabled = autoCreateVms;
            chkAutoCreateStartMenuShortcut.IsEnabled = autoCreateVms;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;

                var appSettings = await _settingsProvider.Get();

                chkStartOnLogin.IsChecked = appSettings.StartOnLogin;
                chkNotifyOnNewVm.IsChecked = appSettings.NotifyOnNewVm;
                chkAutoDeleteShortcuts.IsChecked = appSettings.AutoDeleteShortcuts;
                chkAutoCreateShortcuts.IsChecked = appSettings.AutoCreateShortcuts;
                chkAutoCreateDesktopShortcut.IsChecked = appSettings.AutoCreateDesktopShortcut;
                chkAutoCreateStartMenuShortcut.IsChecked = appSettings.AutoCreateStartMenuShortcut;

                EnableControls();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void OnCheckboxStateChanged(object sender, RoutedEventArgs e)
        {
            await HandleSettingChange();
            EnableControls();
        }

        private async Task HandleSettingChange()
        {
            if (_isLoading)
            {
                return;
            }

            var appSettings = await _settingsProvider.Get(true);

            appSettings.StartOnLogin = chkStartOnLogin.IsChecked.GetValueOrDefault();
            appSettings.NotifyOnNewVm = chkNotifyOnNewVm.IsChecked.GetValueOrDefault();
            appSettings.AutoCreateShortcuts = chkAutoCreateShortcuts.IsChecked.GetValueOrDefault();
            appSettings.AutoDeleteShortcuts = chkAutoDeleteShortcuts.IsChecked.GetValueOrDefault();
            appSettings.AutoCreateDesktopShortcut = chkAutoCreateDesktopShortcut.IsChecked.GetValueOrDefault();
            appSettings.AutoCreateStartMenuShortcut = chkAutoCreateStartMenuShortcut.IsChecked.GetValueOrDefault();

            if (appSettings.StartOnLogin)
            {
                _shortcutProvider.CreateStartupShortcut();
            }
            else
            {
                _shortcutProvider.DeleteStartupShortcut();
            }

            await _settingsProvider.Save();
            await _trayIpcProvider.SendReloadSettings();
        }
    }
}

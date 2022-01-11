using System.Windows;
using System.Windows.Controls;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Pages
{
    public partial class SettingsPage : Page
    {
        private bool _isLoading;

        private AppSettings? _appSettings;

        private readonly IShortcutProvider _shortcutProvider;
        private readonly ISettingsProvider _settingsProvider;

        public SettingsPage(
            IShortcutProvider shortcutProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _shortcutProvider = shortcutProvider;
            _settingsProvider = settingsProvider;
        }

        private void EnableControls()
        {
            if (_appSettings is null)
            {
                return;
            }

            chkNotifyOnNewVm.IsEnabled = !_appSettings.AutoCreateShortcuts;

            chkAutoCreateDesktopShortcut.IsEnabled = _appSettings.AutoCreateShortcuts;
            chkAutoCreateStartMenuShortcut.IsEnabled = _appSettings.AutoCreateShortcuts;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;

                _appSettings = await _settingsProvider.Get();

                chkStartOnLogin.IsChecked = _appSettings.StartOnLogin;
                chkNotifyOnNewVm.IsChecked = _appSettings.NotifyOnNewVm;
                chkAutoDeleteShortcuts.IsChecked = _appSettings.AutoDeleteShortcuts;
                chkAutoCreateShortcuts.IsChecked = _appSettings.AutoCreateShortcuts;
                chkAutoCreateDesktopShortcut.IsChecked = _appSettings.AutoCreateDesktopShortcut;
                chkAutoCreateStartMenuShortcut.IsChecked = _appSettings.AutoCreateStartMenuShortcut;

                EnableControls();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void OnCheckboxStateChanged(object sender, RoutedEventArgs e)
        {
            HandleSettingChange();
            EnableControls();
        }

        private void HandleSettingChange()
        {
            if (_appSettings is null || _isLoading)
            {
                return;
            }

            _appSettings.StartOnLogin = chkStartOnLogin.IsChecked.GetValueOrDefault();
            _appSettings.NotifyOnNewVm = chkNotifyOnNewVm.IsChecked.GetValueOrDefault();
            _appSettings.AutoCreateShortcuts = chkAutoCreateShortcuts.IsChecked.GetValueOrDefault();
            _appSettings.AutoDeleteShortcuts = chkAutoDeleteShortcuts.IsChecked.GetValueOrDefault();
            _appSettings.AutoCreateDesktopShortcut = chkAutoCreateDesktopShortcut.IsChecked.GetValueOrDefault();
            _appSettings.AutoCreateStartMenuShortcut = chkAutoCreateStartMenuShortcut.IsChecked.GetValueOrDefault();

            if (_appSettings.StartOnLogin)
            {
                _shortcutProvider.CreateStartupShortcut();
            }
            else
            {
                _shortcutProvider.DeleteStartupShortcut();
            }

            _settingsProvider.Save();
        }
    }
}

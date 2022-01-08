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
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;

                _appSettings = await _settingsProvider.Get();

                chkStartOnLogin.IsChecked = _appSettings.StartOnLogin;
                chkNotifyOnNewVm.IsChecked = _appSettings.NotifyOnNewVm;
                chkAutoCreateShortcuts.IsChecked = _appSettings.AutoCreateShortcuts;

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

        private bool GetCheckboxValue(CheckBox checkBox)
        {
            if (!checkBox.IsChecked.HasValue)
            {
                return false;
            }

            return checkBox.IsChecked.Value;
        }

        private void HandleSettingChange()
        {
            if (_appSettings is null || _isLoading)
            {
                return;
            }

            _appSettings.StartOnLogin = GetCheckboxValue(chkStartOnLogin);
            _appSettings.NotifyOnNewVm = GetCheckboxValue(chkNotifyOnNewVm);
            _appSettings.AutoCreateShortcuts = GetCheckboxValue(chkAutoCreateShortcuts);

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

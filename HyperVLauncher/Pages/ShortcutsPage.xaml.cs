using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Controls;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.HyperV;

using HyperVLauncher.Modals;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for ShortcutsPage.xaml
    /// </summary>
    public partial class ShortcutsPage : Page
    {
        private readonly ISettingsProvider _settingsProvider;

        private readonly ObservableCollection<Shortcut> _shortcuts = new();

        public ShortcutsPage(
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _settingsProvider = settingsProvider;

            lstShortcuts.ItemsSource = _shortcuts;
        }

        private async Task RefreshShortcuts()
        {
            _shortcuts.Clear();

            var appSettings = await _settingsProvider.Get();

            foreach (var shortcut in appSettings.Shortcuts)
            {
                _shortcuts.Add(shortcut);
            }

            EnableControls();
        }

        private void EnableControls()
        {
            var enable = lstShortcuts.SelectedIndex != -1;

            btnEdit.IsEnabled = enable;
            btnLaunch.IsEnabled = enable;
            btnDelete.IsEnabled = enable;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshShortcuts();
        }

        private void lstShortcuts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete shortcut {shortcut.VmName}?",
                "Hyper-V Launcher - Delete shortcut",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var appSettings = await _settingsProvider.Get();
            
            appSettings.DeleteShortcut(shortcut.Id);

            await _settingsProvider.Save();

            await RefreshShortcuts();
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var vmName = shortcut.VmName;

            HyperVProvider.StartVirtualMachine(vmName);
            HyperVProvider.ConnectVirtualMachine(vmName);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var shortcutWindow = new ShortcutWindow(true, shortcut);
            shortcutWindow.ShowDialog();
        }
    }
}

using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for ShortcutsPage.xaml
    /// </summary>
    public partial class ShortcutsPage : Page
    {
        private readonly ISettingsProvider _settingsProvider;

        private readonly List<Shortcut> _shortcuts = new();

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


            {
                foreach (var shortcut in appSettings.Shortcuts)
                {
                    _shortcuts.Add(shortcut);
                }

                EnableControls();
            }
        }

        private void EnableControls()
        {
            var enable = lstShortcuts.SelectedIndex != -1;

            btnEdit.IsEnabled = enable;
            btnLaunch.IsEnabled = enable;
            btnDelete.IsEnabled = enable;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await RefreshShortcuts();
        }

        private void lstShortcuts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }
    }
}

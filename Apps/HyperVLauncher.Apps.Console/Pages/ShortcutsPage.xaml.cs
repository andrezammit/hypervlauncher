using System;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Modals;
using HyperVLauncher.Providers.Common;

namespace HyperVLauncher.Pages
{
    internal class ShortcutItem : Shortcut
    {
        private readonly IHyperVProvider _hyperVProvider;

        public ShortcutItem(
            Shortcut shortcut,
            IHyperVProvider hyperVProvider)
            : base(
                shortcut.Id,
                shortcut.VmId,
                shortcut.Name,
                shortcut.CloseAction)
        {
            _hyperVProvider = hyperVProvider;
        }

        public string VmName
        {
            get
            {
                try
                {
                    return _hyperVProvider.GetVirtualMachineName(VmId);
                }
                catch
                {
                }

                return "Virtual Machine not found";
            }
        }
    }

    public class ShortcutTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? VmNameTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (DefaultTemplate is null ||
                VmNameTemplate is null)
            {
                throw new InvalidOperationException("DataTemplates are not initialized.");
            }

            var selectedTemplate = DefaultTemplate;

            if (item is not null and ShortcutItem shortcutItem)
            {
                if (shortcutItem.Name != shortcutItem.VmName)
                {
                    selectedTemplate = VmNameTemplate;
                }
            }

            return selectedTemplate;
        }
    }

    public partial class ShortcutsPage : Page
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ITrayIpcProvider _trayIpcProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IShortcutProvider _shortcutProvider;
        private readonly ILaunchPadIpcProvider _launchPadIpcProvider;

        private readonly ObservableCollection<ShortcutItem> _shortcuts = new();

        public ShortcutsPage(
            IHyperVProvider hyperVProvider,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            ISettingsProvider settingsProvider,
            ILaunchPadIpcProvider launchPadIpcProvider)
        {
            InitializeComponent();

            _hyperVProvider = hyperVProvider;
            _trayIpcProvider = trayIpcProvider;
            _settingsProvider = settingsProvider;
            _shortcutProvider = shortcutProvider;
            _launchPadIpcProvider = launchPadIpcProvider;

            lstShortcuts.ItemsSource = _shortcuts;
        }

        public async Task RefreshShortcuts()
        {
            _shortcuts.Clear();

            var appSettings = await _settingsProvider.Get(true);

            foreach (var shortcut in appSettings.Shortcuts)
            {
                _shortcuts.Add(new ShortcutItem(shortcut, _hyperVProvider));
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

        private void LstShortcuts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            await DeleteSelectedShortcuts();
        }

        private async Task DeleteSelectedShortcuts()
        {
            if (lstShortcuts.SelectedItems.Count == 0)
            {
                return;
            }

            var shortcutsToDelete = new List<Shortcut>();

            foreach (var selectedItem in lstShortcuts.SelectedItems)
            {
                if (selectedItem is Shortcut shortcut)
                {
                    shortcutsToDelete.Add(shortcut);
                }
            }

            MessageBoxResult messageBoxResult;

            if (shortcutsToDelete.Count == 1)
            {
                var shortcut = shortcutsToDelete.First();

                messageBoxResult = MessageBox.Show(
                    $"Are you sure you want to delete shortcut {shortcut.Name}?",
                    GeneralConstants.AppName,
                    MessageBoxButton.YesNo);
            }
            else
            {
                messageBoxResult = MessageBox.Show(
                    $"Are you sure you want to delete {shortcutsToDelete.Count} shortcuts?",
                    GeneralConstants.AppName,
                    MessageBoxButton.YesNo);
            }

            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                foreach (var shortcut in shortcutsToDelete)
                {
                    await _settingsProvider.ProcessDeleteShortcut(
                        shortcut,
                        _trayIpcProvider,
                        _shortcutProvider);
                }

                await RefreshShortcuts();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            GenericHelpers.LaunchShortcut(
                shortcut.Id,
                _launchPadIpcProvider);
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var shortcutWindow = new ShortcutWindow(
                true, 
                shortcut, 
                _hyperVProvider,
                _settingsProvider);

            if (shortcutWindow.ShowDialog() is not null and false)
            {
                return;
            }

            var appSettings = await _settingsProvider.Get(true);

            var savedShortcut = appSettings.Shortcuts.FirstOrDefault(x => x.Id == shortcut.Id);

            if (savedShortcut is null)
            {
                return;
            }

            savedShortcut.Name = shortcutWindow.txtName.Text;
            savedShortcut.CloseAction = shortcutWindow.GetSelectedCloseAction();

            await _settingsProvider.Save();

            await RefreshShortcuts();
            
            await _trayIpcProvider.SendReloadSettings();
        }

        private async void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                await DeleteSelectedShortcuts();
            }
        }
    }
}

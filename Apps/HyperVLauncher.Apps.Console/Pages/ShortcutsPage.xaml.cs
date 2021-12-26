using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Controls;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Modals;

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

        public string VmName => _hyperVProvider.GetVmName(VmId);
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

    /// <summary>
    /// Interaction logic for ShortcutsPage.xaml
    /// </summary>
    public partial class ShortcutsPage : Page
    {
        private readonly IIpcProvider _ipcProvider;
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ISettingsProvider _settingsProvider;

        private readonly ObservableCollection<ShortcutItem> _shortcuts = new();

        public ShortcutsPage(
            IIpcProvider ipcProvider,
            IHyperVProvider hyperVProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _ipcProvider = ipcProvider;
            _hyperVProvider = hyperVProvider;
            _settingsProvider = settingsProvider;

            lstShortcuts.ItemsSource = _shortcuts;
        }

        private async Task RefreshShortcuts()
        {
            _shortcuts.Clear();

            var appSettings = await _settingsProvider.Get();

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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshShortcuts();
        }

        private void LstShortcuts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete shortcut {shortcut.Name}?",
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

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var vmId = shortcut.VmId;

            _hyperVProvider.StartVirtualMachine(vmId);
            _hyperVProvider.ConnectVirtualMachine(vmId);
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstShortcuts.SelectedItem is not Shortcut shortcut)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var shortcutWindow = new ShortcutWindow(true, shortcut, _hyperVProvider);

            if (shortcutWindow.ShowDialog() is not null and false)
            {
                return;
            }

            var appSettings = await _settingsProvider.Get();

            var savedShortcut = appSettings.Shortcuts.FirstOrDefault(x => x.Id == shortcut.Id);

            if (savedShortcut is null)
            {
                return;
            }

            savedShortcut.Name = shortcutWindow.txtName.Text;
            savedShortcut.CloseAction = shortcutWindow.GetSelectedCloseAction();

            await _settingsProvider.Save();

            await RefreshShortcuts();

            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ReloadSettings
            };

            await _ipcProvider.SendMessage(ipcMessage);
        }
    }
}

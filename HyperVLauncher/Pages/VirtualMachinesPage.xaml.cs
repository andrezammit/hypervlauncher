using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Modals;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for VirtualMachinesPage.xaml
    /// </summary>
    public partial class VirtualMachinesPage : Page
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ISettingsProvider _settingsProvider;

        private readonly ObservableCollection<VirtualMachine> _virtualMachines = new();

        public VirtualMachinesPage(
            IHyperVProvider hyperVProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _hyperVProvider = hyperVProvider;
            _settingsProvider = settingsProvider;

            lstVirtualMachines.ItemsSource = _virtualMachines;

            RefreshVirtualMachines();
        }

        private void RefreshVirtualMachines()
        {
            _virtualMachines.Clear();

            var vmList = _hyperVProvider.GetVirtualMachineList();
            
            foreach (var vm in vmList)
            {
                _virtualMachines.Add(vm);
            }

            EnableControls();
        }

        private void lstVirtualMachines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }

        private void EnableControls()
        {
            var enable = lstVirtualMachines.SelectedIndex != -1;

            btnLaunch.IsEnabled = enable;
            btnCreateShortcut.IsEnabled = enable;
        }

        private void btnLaunch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstVirtualMachines.SelectedItem is not VirtualMachine vm)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var vmName = vm.Name;

            _hyperVProvider.StartVirtualMachine(vmName);
            _hyperVProvider.ConnectVirtualMachine(vmName);
        }

        private async void btnCreateShortcut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstVirtualMachines.SelectedItem is not VirtualMachine vm)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }
            
            var shortcut = AppSettings.CreateShortcut(vm.Name, vm.Id);

            var shortcutWindow = new ShortcutWindow(false, shortcut, _hyperVProvider);

            if (shortcutWindow.ShowDialog() is not null and false)
            {
                return;
            }

            shortcut.Name = shortcutWindow.txtName.Text;

            var appSettings = await _settingsProvider.Get();
            appSettings.Shortcuts.Add(shortcut);

            await _settingsProvider.Save();
        }
    }
}

using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.HyperV;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for VirtualMachinesPage.xaml
    /// </summary>
    public partial class VirtualMachinesPage : Page
    {
        private readonly ISettingsProvider _settingsProvider;

        private readonly ObservableCollection<VirtualMachine> _virtualMachines = new();

        public VirtualMachinesPage(ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _settingsProvider = settingsProvider;

            lstVirtualMachines.ItemsSource = _virtualMachines;

            RefreshVirtualMachines();
        }

        private void RefreshVirtualMachines()
        {
            _virtualMachines.Clear();

            var vmList = HyperVProvider.GetVirtualMachineList();
            
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
         
            HyperVProvider.StartVirtualMachine(vmName);
            HyperVProvider.ConnectVirtualMachine(vmName);
        }

        private async void btnCreateShortcut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstVirtualMachines.SelectedItem is not VirtualMachine vm)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            var vmName = vm.Name;
            var appSettings = await _settingsProvider.Get();

            appSettings.AddShortcut(vmName, vmName);

            await _settingsProvider.Save();
        }
    }
}

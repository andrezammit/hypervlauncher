using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Providers.HyperV;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for VirtualMachinesPage.xaml
    /// </summary>
    public partial class VirtualMachinesPage : Page
    {
        private readonly List<VirtualMachine> _virtualMachines = new();

        public VirtualMachinesPage()
        {
            InitializeComponent();

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
            StartVm(vmName);

            var startInfo = new ProcessStartInfo("vmconnect.exe", $"localhost \"{vmName}\"");
            
            using (Process.Start(startInfo))
            {

            }
        }

        private static void StartVm(string vmName)
        {
            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new SelectQuery($"SELECT * FROM Msvm_ComputerSystem WHERE ElementName = \"{vmName}\"");

            using var searcher = new ManagementObjectSearcher(scope, query);
            using var searchResults = searcher.Get();

            foreach (ManagementObject vmObject in searchResults)
            {
                var inParams = vmObject.GetMethodParameters("RequestStateChange");

                inParams["RequestedState"] = 2;

                var outParams = vmObject.InvokeMethod(
                    "RequestStateChange",
                    inParams,
                    null);
            }
        }
    }
}

using System.Management;
using System.Windows.Controls;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Models;

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

            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem");

            using var searcher = new ManagementObjectSearcher(scope, query);

            foreach (var queryObj in searcher.Get())
            {
                var vmName = queryObj["ElementName"].ToString();

                if (!string.IsNullOrEmpty(vmName))
                {
                    _virtualMachines.Add(new VirtualMachine(vmName));
                }
            }
        }
    }
}

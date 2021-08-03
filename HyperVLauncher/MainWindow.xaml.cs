using System.Windows;
using System.Management;
using System.Collections.Generic;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double _navPanelOriginalWidth;

        private readonly List<VirtualMachine> _virtualMachines = new();

        private bool _navPanelShowing = true;


        public MainWindow()
        {
            InitializeComponent();

            _navPanelOriginalWidth = navPanel.Width;

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

        private void btnBurger_Click(object sender, RoutedEventArgs e)
        {
            navPanel.Width = _navPanelShowing ? 50 : _navPanelOriginalWidth;

            _navPanelShowing = !_navPanelShowing;
        }
    }

    internal class VirtualMachine
    {
        public string Name { get; init; }

        public VirtualMachine(string name)
        {
            Name = name;
        }
    }
}

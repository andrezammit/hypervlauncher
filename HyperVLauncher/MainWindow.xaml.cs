using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<VirtualMachine> _virtualMachines = new();

        public MainWindow()
        {
            InitializeComponent();

            lstVirtualMachines.ItemsSource = _virtualMachines;

            RefreshVirtualMachines();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

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

    internal class VirtualMachine
    {
        public string Name { get; init; }

        public VirtualMachine(string name)
        {
            Name = name;
        }
    }
}

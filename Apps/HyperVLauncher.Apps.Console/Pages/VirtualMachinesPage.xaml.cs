using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

using HyperVLauncher.Modals;

namespace HyperVLauncher.Pages
{
    /// <summary>
    /// Interaction logic for VirtualMachinesPage.xaml
    /// </summary>
    public partial class VirtualMachinesPage : Page
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ITrayIpcProvider _trayIpcProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IShortcutProvider _shortcutProvider;

        private readonly ObservableCollection<VirtualMachine> _virtualMachines = new();

        public VirtualMachinesPage(
            IHyperVProvider hyperVProvider,
            ITrayIpcProvider trayIpcProvider,
            IShortcutProvider shortcutProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _hyperVProvider = hyperVProvider;
            _trayIpcProvider = trayIpcProvider;
            _settingsProvider = settingsProvider;
            _shortcutProvider = shortcutProvider;

            lstVirtualMachines.ItemsSource = _virtualMachines;

            RefreshVirtualMachines();
        }

        private void RefreshVirtualMachines()
        {
            Tracer.Debug("Refresing virtual machine list...");

            _virtualMachines.Clear();

            try
            {
                var vmList = _hyperVProvider.GetVirtualMachineList();

                foreach (var vm in vmList)
                {
                    Tracer.Debug($"Found virtual machine: {vm.Id} - {vm.Name}");

                    _virtualMachines.Add(vm);
                }
            }
            catch
            {
                // Failed to get VM list from Hyper-V.
            }

            if (_virtualMachines.Count == 0)
            {
                _virtualMachines.Add(new VirtualMachine(Guid.Empty.ToString(), "No Virtual Machines found."));
            }

            EnableControls();
        }

        private void LstVirtualMachines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableControls();
        }

        private void EnableControls()
        {
            var enable = (lstVirtualMachines.SelectedItem is VirtualMachine vm)
                && (vm.Id != Guid.Empty.ToString());

            btnLaunch.IsEnabled = enable;
            btnCreateShortcut.IsEnabled = enable;
        }

        private void BtnLaunch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstVirtualMachines.SelectedItem is not VirtualMachine vm)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            Tracer.Debug($"Launching virtual machine {vm.Id} - {vm.Name}...");

            var vmId = vm.Id;

            var vmState = _hyperVProvider.GetVirtualMachineState(vmId);

            if (vmState == VmState.Saved || vmState == VmState.Stopped)
            {
                Tracer.Debug($"Starting {vm.Name}...");

                _hyperVProvider.StartVirtualMachine(vmId);
            }

            Tracer.Debug($"Connecting to {vm.Name}...");

            using var process = _hyperVProvider.ConnectVirtualMachine(vmId);
        }

        private async void BtnCreateShortcut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstVirtualMachines.SelectedItem is not VirtualMachine vm)
            {
                throw new InvalidCastException("Invalid selected item type.");
            }

            Tracer.Debug($"Creating new shortcut for {vm.Id} - {vm.Name}...");

            var shortcut = AppSettings.CreateShortcut(vm.Name, vm.Id);

            var shortcutWindow = new ShortcutWindow(
                false, 
                shortcut, 
                _hyperVProvider, 
                _settingsProvider);

            if (shortcutWindow.ShowDialog() is not null and false)
            {
                return;
            }

            shortcut.Name = shortcutWindow.txtName.Text;

            var appSettings = await _settingsProvider.Get();
            appSettings.Shortcuts.Add(shortcut);

            await _settingsProvider.Save();

            Tracer.Info($"New shortcut \"{shortcut.Name}\" created for {vm.Id} - {vm.Name}.");

            if (shortcutWindow.chkDesktopShortcut.IsChecked.HasValue && 
                shortcutWindow.chkDesktopShortcut.IsChecked.Value)
            {
                Tracer.Info($"Creating desktop shortcut for \"{shortcut.Name}\"...");

                _shortcutProvider.CreateDesktopShortcut(shortcut);
            }

            if (shortcutWindow.chkStartMenuShortcut.IsChecked.HasValue &&
                shortcutWindow.chkStartMenuShortcut.IsChecked.Value)
            {
                Tracer.Info($"Creating start menu shortcut for \"{shortcut.Name}\"...");

                _shortcutProvider.CreateStartMenuShortcut(shortcut);
            }

            await _trayIpcProvider.SendReloadSettings();
            await _trayIpcProvider.SendShowMessageNotif("New Shortcut Created", $"Your new virtual machine shortcut \"{shortcut.Name}\" is now accessible by clicking on the system tray icon.");
        }
    }
}

using System.Windows;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Common;

namespace HyperVLauncher.Modals
{
    public partial class ShortcutWindow : Window
    {
        private readonly string _shortcutId;

        private readonly ISettingsProvider _settingsProvider;

        public ShortcutWindow(
            bool editMode,
            Shortcut shortcut,
            IHyperVProvider hyperVProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _shortcutId = shortcut.Id;
            _settingsProvider = settingsProvider;

            string vmName;

            try
            {
                vmName = hyperVProvider.GetVirtualMachineName(shortcut.VmId);
            }
            catch
            {
                vmName = "Virtual Machine not found";
            }

            if (editMode)
            {
                txtName.Text = shortcut.Name;
            }
            else
            {
                var appSettings = _settingsProvider.Get(true)
                    .GetAwaiter()
                    .GetResult();

                txtName.Text = _settingsProvider.GetValidShortcutName(_shortcutId, vmName, appSettings);
            }

            lblVmName.Content = vmName;

            var closeActionItems = new List<CloseActionItem>()
            {
                new CloseActionItem(CloseAction.None, "Leave in current state"),
                new CloseActionItem(CloseAction.Pause, "Pause the Virtual Machine"),
                new CloseActionItem(CloseAction.Shutdown, "Shutdown the Virtual Machine")
            };

            PopulateCloseActionComboBox(closeActionItems);

            if (editMode)
            {
                lblTitle.Content = "Edit Shortcut";
                
                stackPanel.Children.Remove(chkDesktopShortcut);
                stackPanel.Children.Remove(chkStartMenuShortcut);

                SetDefaultCloseActionSelection(shortcut.CloseAction);

                txtRdpPort.Text = shortcut.RdpPort.ToString();
                chkRdpTrigger.IsChecked = shortcut.RdpTriggerEnabled;
            }
            else
            {
                SetDefaultCloseActionSelection(CloseAction.None);

            }

            if (!chkRdpTrigger.IsChecked.GetValueOrDefault())
            {
                txtRdpPort.Text = GenericHelpers.GetAvailablePort(3390).ToString();
            }

            EnableControls();
        }

        private void EnableControls()
        {
            txtRdpPort.IsEnabled = chkRdpTrigger.IsChecked.GetValueOrDefault();
        }

        public CloseAction GetSelectedCloseAction()
        {
            var closeActionItem = (CloseActionItem) cmbCloseAction.SelectedItem;

            return closeActionItem.CloseAction;
        }

        private void PopulateCloseActionComboBox(IEnumerable<CloseActionItem> closeActionItems)
        {
            foreach (var closeActionItem in closeActionItems)
            {
                cmbCloseAction.Items.Add(closeActionItem);
            }
        }

        private void SetDefaultCloseActionSelection(CloseAction closeAction)
        {
            cmbCloseAction.SelectedIndex = GetComboBoxIndexFromCloseAction(closeAction);
        }

        private int GetComboBoxIndexFromCloseAction(CloseAction closeAction)
        {
            foreach (CloseActionItem comboBoxItem in cmbCloseAction.Items)
            {
                if (comboBoxItem.CloseAction == closeAction)
                {
                    return cmbCloseAction.Items.IndexOf(comboBoxItem);
                }    
            }

            return -1;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var appSettings = await _settingsProvider.Get(true);

            if (!_settingsProvider.ValidateShortcutName(
                _shortcutId,
                txtName.Text,
                appSettings))
            {
                MessageBox.Show(
                    $"Another shortcut is already named \"{txtName.Text}\".", 
                    GeneralConstants.AppName, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);

                txtName.Focus();
                return;
            }

            DialogResult = true;
        }

        private void ChkRdpTrigger_OnCheckChange(object sender, RoutedEventArgs e)
        {
            EnableControls();
        }
    }
}

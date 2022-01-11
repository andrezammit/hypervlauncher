using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

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
                txtName.Text = _settingsProvider.GetValidShortcutName(_shortcutId, vmName)
                    .GetAwaiter()
                    .GetResult();
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
            }
            else
            {
                SetDefaultCloseActionSelection(CloseAction.None);
            }
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
            if (!await _settingsProvider.ValidateShortcutName(
                _shortcutId,
                txtName.Text))
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
    }
}

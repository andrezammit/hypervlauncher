using System.Windows;
using System.Collections.Generic;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Modals
{
    /// <summary>
    /// Interaction logic for ShortcutWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        public ShortcutWindow(
            bool editMode,
            Shortcut shortcut,
            IHyperVProvider hyperVProvider)
        {
            InitializeComponent();

            txtName.Text = shortcut.Name;
            lblVmName.Content = hyperVProvider.GetVmName(shortcut.VmId);

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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

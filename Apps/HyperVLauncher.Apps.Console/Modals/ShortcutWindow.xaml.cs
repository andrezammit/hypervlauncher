using System.Windows;

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

            if (editMode)
            {
                lblTitle.Content = "Edit Shortcut";

                stackPanel.Children.Remove(chkDesktopShortcut);
                stackPanel.Children.Remove(chkStartMenuShortcut);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

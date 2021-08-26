using System.Windows;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Modals
{
    /// <summary>
    /// Interaction logic for ShortcutWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        private Shortcut _shortcut;

        public ShortcutWindow(
            bool editMode,
            Shortcut shortcut)
        {
            InitializeComponent();

            _shortcut = shortcut;

            txtName.Text = shortcut.Name;
            lblVmName.Content = shortcut.VmName;

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
    }
}

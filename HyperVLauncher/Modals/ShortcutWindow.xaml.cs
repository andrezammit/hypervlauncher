using System.Windows;

using MahApps.Metro.Controls;

namespace HyperVLauncher.Modals
{
    /// <summary>
    /// Interaction logic for ShortcutWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        public ShortcutWindow()
        {
            InitializeComponent();
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleSwitch toggleSwitch)
            {
                return;
            }

            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                }
                else
                {
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

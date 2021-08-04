using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using HyperVLauncher.Pages;
using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double _navPanelOriginalWidth;

        private readonly Dictionary<MainPages, Page> _pages = new();

        private bool _navPanelShowing = true;

        public MainWindow()
        {
            InitializeComponent();

            CreatePages();

            _navPanelOriginalWidth = navPanel.Width;

            pageFrame.NavigationService.Navigate(_pages[MainPages.VirtualMachines]);
        }

        private void CreatePages()
        {
            _pages[MainPages.Shortcuts] = new ShortcutsPage();
            _pages[MainPages.VirtualMachines] = new VirtualMachinesPage();
        }

        private void btnBurger_Click(object sender, RoutedEventArgs e)
        {
            navPanel.Width = _navPanelShowing ? 50 : _navPanelOriginalWidth;

            _navPanelShowing = !_navPanelShowing;
        }

        private void btnShortcuts_Click(object sender, RoutedEventArgs e)
        {
            pageFrame.NavigationService.Navigate(_pages[MainPages.Shortcuts]);
        }

        private void btnVirtualMachines_Click(object sender, RoutedEventArgs e)
        {
            pageFrame.NavigationService.Navigate(_pages[MainPages.VirtualMachines]);
        }
    }
}

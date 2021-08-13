using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

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

        private readonly IServiceProvider _serviceProvider;

        private readonly List<Button> _navButtons = new();
        private readonly Dictionary<MainPages, Page> _pages = new();

        private bool _navPanelShowing = true;

        public MainWindow(IServiceProvider serviceProvider)
{
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _navPanelOriginalWidth = navPanel.Width;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePages();
            CacheNavButtons();

            SetSelectedNavButton(btnVirtualMachines);
            pageFrame.NavigationService.Navigate(_pages[MainPages.VirtualMachines]);
        }

        private void CreatePages()
        {
            _pages[MainPages.Shortcuts] = _serviceProvider.GetRequiredService<ShortcutsPage>();
            _pages[MainPages.VirtualMachines] = _serviceProvider.GetRequiredService<VirtualMachinesPage>();
        }

        private void CacheNavButtons()
        {
            _navButtons.Add(btnShortcuts);
            _navButtons.Add(btnVirtualMachines);
            _navButtons.Add(btnSettings);
        }

        private void btnBurger_Click(object sender, RoutedEventArgs e)
        {
            navPanel.Width = _navPanelShowing ? 50 : _navPanelOriginalWidth;

            _navPanelShowing = !_navPanelShowing;
        }

        private void btnShortcuts_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.NavigationService.Navigate(_pages[MainPages.Shortcuts]);
        }

        private void btnVirtualMachines_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.NavigationService.Navigate(_pages[MainPages.VirtualMachines]);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.NavigationService.Navigate(_pages[MainPages.VirtualMachines]);
        }

        private void SetSelectedNavButton(Button selectedButton)
        {
            foreach (var navButton in _navButtons)
            {
                var navButtonStyle = Resources["NavButton"] as Style;

                if (navButton == selectedButton)
                {
                    navButtonStyle = Resources["SelectedNavButton"] as Style;
                }

                navButton.Style = navButtonStyle;
            }
        }
    }
}

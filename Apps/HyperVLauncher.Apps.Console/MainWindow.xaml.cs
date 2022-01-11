using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Microsoft.Extensions.DependencyInjection;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Pages;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double _navPanelOriginalWidth;

        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsProvider _settingsProvider;

        private readonly List<Button> _navButtons = new();
        private readonly Dictionary<MainPages, Page> _pages = new();

        private bool _navPanelShowing = true;

        public MainWindow(
            IServiceProvider serviceProvider,
            ISettingsProvider settingsProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _settingsProvider = settingsProvider;

            _navPanelOriginalWidth = navPanel.Width;

            pageFrame.Navigated += PageFrame_Navigated;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePages();
            CacheNavButtons();

            await NavigateToInitialPage();
        }

        private void CreatePages()
        {
            _pages[MainPages.Shortcuts] = _serviceProvider.GetRequiredService<ShortcutsPage>();
            _pages[MainPages.VirtualMachines] = _serviceProvider.GetRequiredService<VirtualMachinesPage>();
            _pages[MainPages.Settings] = _serviceProvider.GetRequiredService<SettingsPage>();
        }

        private void CacheNavButtons()
        {
            _navButtons.Add(btnShortcuts);
            _navButtons.Add(btnVirtualMachines);
            _navButtons.Add(btnSettings);
        }

        private async Task NavigateToInitialPage()
        {
            var appSettings = await _settingsProvider.Get();
            var anyShortcuts = appSettings.Shortcuts.Any();

            if (anyShortcuts)
            {
                SetSelectedNavButton(btnShortcuts);
                pageFrame.Navigate(_pages[MainPages.Shortcuts]);
            }
            else
            {
                SetSelectedNavButton(btnVirtualMachines);
                pageFrame.Navigate(_pages[MainPages.VirtualMachines]);
            }
        }

        private async void PageFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is VirtualMachinesPage virtualMachinesPage)
            {
                virtualMachinesPage.RefreshVirtualMachines();
            }
            else if (e.Content is ShortcutsPage shortcutsPage)
            {
                await shortcutsPage.RefreshShortcuts();
            }
        }

        private void BtnBurger_Click(object sender, RoutedEventArgs e)
        {
            navPanel.Width = _navPanelShowing ? 50 : _navPanelOriginalWidth;

            _navPanelShowing = !_navPanelShowing;
        }

        private void BtnShortcuts_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.Navigate(_pages[MainPages.Shortcuts]);
        }

        private void BtnVirtualMachines_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.Navigate(_pages[MainPages.VirtualMachines]);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button navButton)
            {
                return;
            }

            SetSelectedNavButton(navButton);

            pageFrame.Navigate(_pages[MainPages.Settings]);
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

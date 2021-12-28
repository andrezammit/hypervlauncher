using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Settings;

using HyperVLauncher.Pages;
using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Contracts.Constants;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            if (!GenericHelpers.IsUniqueInstance("HyperVLauncherConsoleMutex"))
            {
                base.Shutdown();

                return;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var profilePath = PathProvider.GetProfileFolder();
            Directory.CreateDirectory(profilePath);

            services.AddSingleton<MainWindow>();
            services.AddSingleton<ShortcutsPage>();
            services.AddSingleton<VirtualMachinesPage>();
            
            services.AddSingleton<IHyperVProvider, HyperVProvider>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
         
            services.AddSingleton<IPathProvider>(provider => new PathProvider(profilePath));
            services.AddSingleton<IIpcProvider>(provider => new IpcProvider(GeneralConstants.IpcPipeName));
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            LaunchTrayApp();
        }

        private static void LaunchTrayApp()
        {
            var startInfo = new ProcessStartInfo($"{AppContext.BaseDirectory}\\HyperVLauncher.Apps.Tray.exe");

            using (Process.Start(startInfo))
            {
            }
        }
    }
}
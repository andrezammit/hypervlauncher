using System;
using System.IO;
using System.Windows;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Providers.Settings;

using HyperVLauncher.Pages;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IPathProvider _pathProvider;
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var profilePath = PathProvider.GetProfileFolder();
            Directory.CreateDirectory(profilePath);

            var tracingPath = Path.Combine(profilePath, "Logs");
            Directory.CreateDirectory(tracingPath);

            _pathProvider = new PathProvider(profilePath);

            TracingProvider.Init(tracingPath, "Console");

            Tracer.Debug("Starting Console...");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            if (!GenericHelpers.IsUniqueInstance("HyperVLauncherConsoleMutex"))
            {
                base.Shutdown();

                return;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Tracer.Debug("Closing Console...");

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Tracer.Debug("Setting up DI services...");

            services.AddSingleton<MainWindow>();
            services.AddSingleton<ShortcutsPage>();
            services.AddSingleton<VirtualMachinesPage>();
            
            services.AddSingleton<IHyperVProvider, HyperVProvider>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
         
            services.AddSingleton(provider => _pathProvider);
            services.AddSingleton<IIpcProvider>(provider => new IpcProvider(GeneralConstants.IpcPipeName));
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Tracer.Debug("Showing main window...");

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

#if !DEBUG
            LaunchTrayApp();
#endif
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
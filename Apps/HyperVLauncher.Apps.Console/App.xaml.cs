using System;
using System.Windows;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Ipc;
using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.HyperV;
using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Providers.Settings;
using HyperVLauncher.Providers.Shortcut;

using HyperVLauncher.Pages;

namespace HyperVLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex? _instanceMutex;

        private readonly IPathProvider _pathProvider;
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            _pathProvider = new PathProvider(GeneralConstants.ProfileName);
            _pathProvider.CreateDirectories();

            TracingProvider.Init(_pathProvider.GetTracingPath(), "Console");

            Tracer.Debug("Setting up exception handling...");

            SetupExceptionHandling();

            Tracer.Debug("Starting Console...");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _instanceMutex = GenericHelpers.TakeInstanceMutex(GeneralConstants.ConsoleMutexName);

            if (_instanceMutex is null)
            {
                base.Shutdown();

                return;
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.SetObserved();
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Tracer.Debug("Closing Console...");

            _instanceMutex?.Dispose();

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Tracer.Debug("Setting up DI services...");

            services.AddSingleton<MainWindow>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<ShortcutsPage>();
            services.AddSingleton<VirtualMachinesPage>();
            
            services.AddSingleton<IHyperVProvider, HyperVProvider>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
            services.AddSingleton<IShortcutProvider, ShortcutProvider>();

            services.AddSingleton(provider => _pathProvider);

            var consoleIpcProvider = new IpcProvider(8872);

            services.AddSingleton<ITrayIpcProvider>(consoleIpcProvider);
            services.AddSingleton<IMonitorIpcProvider>(consoleIpcProvider);
            services.AddSingleton<ILaunchPadIpcProvider>(consoleIpcProvider);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Tracer.Debug("Showing main window...");

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void LogUnhandledException(Exception exception)
        {
            Tracer.Error("Unhandled exception.", exception);
        }
    }
}
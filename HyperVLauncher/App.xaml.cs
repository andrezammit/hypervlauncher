using System;
using System.IO;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Path;
using HyperVLauncher.Providers.Settings;

using HyperVLauncher.Pages;

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
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var profilePath = GetProfileFolder();
            Directory.CreateDirectory(profilePath);

            services.AddSingleton<MainWindow>();
            services.AddSingleton<ShortcutsPage>();
            services.AddSingleton<VirtualMachinesPage>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
            services.AddSingleton<IPathProvider>(provider => new PathProvider(profilePath));
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static string GetProfileFolder()
        {
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(programDataPath, "HyperVLauncher");
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoLoTool.ViewModels;
using YoLoTool.Views;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using YoLoTool.Interfaces;
using YoLoTool.Services;
using YoLoTool.AI.Interfaces;
using YoLoTool.AI.Models;
using YoLoTool.Models;

namespace YoLoTool
{
    public partial class App : Application
    {
        private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ICustomPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IYolo, Yolov7>();
            services.AddSingleton<LoaderService>(); 
            services.AddSingleton<ImagesAttributeContainer>();

            services.AddScoped<SettingsView>();
            services.AddScoped<SettingsViewModel>();

            services.AddScoped<ContentView>();
            services.AddScoped<ContentViewModel>();

            services.AddScoped<HomeView>();
            services.AddScoped<HomeViewModel>();

            services.AddScoped<INavigationWindow, MainWindow>();
            services.AddScoped<MainWindowViewModel>();
        }).Build();

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}

using Microsoft.Extensions.Hosting; 
using YoLoTool.Views;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using YoLoTool.Interfaces; 

namespace YoLoTool.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly ICustomPageService _pageService;
    private readonly IThemeService _themeService;

    private INavigationWindow _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider,
        INavigationService navigationService,
        ICustomPageService pageService,
        IThemeService themeService)
    {
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageService = pageService;
        _themeService = themeService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrepareNavigation();

        await HandleActivationAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private async Task HandleActivationAsync()
    {
        await Task.CompletedTask;

        if (!Application.Current.Windows.OfType<MainWindow>().Any())
        {
            _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;
            _navigationWindow!.ShowWindow();

            _navigationWindow.Navigate(typeof(HomeView));
        }

        await Task.CompletedTask;
    }

    private void PrepareNavigation()
    {
        _navigationService.SetPageService(_pageService);
    }
}

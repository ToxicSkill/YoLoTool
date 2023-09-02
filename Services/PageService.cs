using System;
using System.Diagnostics;
using System.Windows;
using YoLoTool.Interfaces;

namespace YoLoTool.Services;

public class PageService : ICustomPageService
{
    private readonly IServiceProvider _serviceProvider;

    public event EventHandler<string> OnPageNavigate;

    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T? GetPage<T>() where T : class
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException("The page should be a WPF control.");

        return (T?)_serviceProvider.GetService(typeof(T));
    }

    public FrameworkElement? GetPage(Type pageType)
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
            Trace.WriteLine("Form is inassingable");
        return _serviceProvider.GetService(pageType) as FrameworkElement;
    }
}

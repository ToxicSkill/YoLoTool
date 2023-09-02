using System;
using Wpf.Ui.Mvvm.Contracts;

namespace YoLoTool.Interfaces
{
    public interface ICustomPageService : IPageService
    {
        event EventHandler<string> OnPageNavigate;
    }
}
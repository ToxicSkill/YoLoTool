﻿using YoLoTool.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using YoLoTool.Interfaces;

namespace YoLoTool
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel
        {
            get;
        }

        public MainWindow(MainWindowViewModel viewModel,
            INavigationService navigationService,
            ICustomPageService pageService,
            ISnackbarService snackbarService)
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();

            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarControl(RootSnackbar);
        }

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void SymbolIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}

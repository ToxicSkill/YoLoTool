using CommunityToolkit.Mvvm.ComponentModel;
using YoLoTool.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Controls.Navigation;

namespace YoLoTool.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public ICollection<INavigationControl> menuItems;

        [ObservableProperty]
        public ICollection<INavigationControl> footerItems;

        public MainWindowViewModel()
        {
            menuItems = new ObservableCollection<INavigationControl>();
            footerItems = new ObservableCollection<INavigationControl>();
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.Home20,
                PageTag = "home",
                Cache = true,
                Content = "Home",
                PageType = typeof(HomeView)
            });
            MenuItems.Add(new NavigationSeparator());
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.SelectObjectSkewEdit20,
                PageTag = "labeling",
                Cache = true,
                Content = "Labeling",
                PageType = typeof(ContentView)
            });
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.ArrowExportUp20,
                PageTag = "export",
                Cache = true,
                Content = "Export",
                PageType = typeof(ExportView)
            });
            FooterItems.Add(new NavigationSeparator());
            FooterItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.Settings20,
                PageTag = "settings",
                Cache = true,
                Content = "Settings",
                PageType = typeof(SettingsView)
            });
        }
    }
}

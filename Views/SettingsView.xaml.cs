using Wpf.Ui.Common.Interfaces;
using YoLoTool.ViewModels;

namespace YoLoTool.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class SettingsView : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel
        {
            get;
        }

        public SettingsView(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}

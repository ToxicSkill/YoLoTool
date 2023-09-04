using Wpf.Ui.Common.Interfaces;
using YoLoTool.ViewModels;

namespace YoLoTool.Views
{
    /// <summary>
    /// Interaction logic for ExportView.xaml
    /// </summary>
    public partial class ExportView : INavigableView<ExportViewModel>
    { 
        public ExportViewModel ViewModel
        {
            get;
        }

        public ExportView(ExportViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}

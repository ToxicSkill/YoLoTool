using YoLoTool.ViewModels;
using Wpf.Ui.Common.Interfaces;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using Wpf.Ui.Controls;

namespace YoLoTool.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class HomeView : INavigableView<HomeViewModel>
    {
        private readonly System.Windows.Thickness InitialThickness = new System.Windows.Thickness(0, 50, 0, 0);

        public HomeViewModel ViewModel
        {
            get;
        }

        public HomeView(HomeViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
            stage1.Margin = InitialThickness;
            stage2.Margin = InitialThickness;
            stage3.Margin = InitialThickness;
        }

        private void Grid_MouseEnterStage1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            switch ((sender as Grid).Name.ToLowerInvariant())
            {
                case "stage1":
                    MariginMouseOverAnimation(stage1, symbolStage1);
                    break;
                case "stage2":
                    MariginMouseOverAnimation(stage2, symbolStage2);
                    break;
                case "stage3":
                    MariginMouseOverAnimation(stage3, symbolStage3);
                    break;
                default:
                    break;
            }
        }

        private void Grid_MouseLeaveStage1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            switch ((sender as Grid).Name.ToLowerInvariant())
            {
                case "stage1":
                    MariginMouseLeavesAnimation(stage1, symbolStage1);
                    break;
                case "stage2":
                    MariginMouseLeavesAnimation(stage2, symbolStage2);
                    break;
                case "stage3":
                    MariginMouseLeavesAnimation(stage3, symbolStage3);
                    break;
                default:
                    break;
            }
        } 
        private void MariginMouseOverAnimation(Grid grid, SymbolIcon symbolIcon)
        {
            symbolIcon.Filled = true;
            ThicknessAnimation da1 = new ()
            {
                From = new System.Windows.Thickness(grid.Margin.Left, grid.Margin.Top, grid.Margin.Left, grid.Margin.Bottom),
                To = new System.Windows.Thickness(0, 0, 0, 0),
                DecelerationRatio = 0.9,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(1)),
            };
            grid.BeginAnimation(Grid.MarginProperty, da1);
        }

        private void MariginMouseLeavesAnimation(Grid grid, SymbolIcon symbolIcon)
        {
            symbolIcon.Filled = false;
            ThicknessAnimation da2 = new ()
            {
                From = new System.Windows.Thickness(grid.Margin.Left, grid.Margin.Top, grid.Margin.Left, grid.Margin.Bottom),
                To = new System.Windows.Thickness(0, 50, 0, 0),
                AccelerationRatio = 0.9,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(1)),

            };
            grid.BeginAnimation(Grid.MarginProperty, da2);
        }
    }
}

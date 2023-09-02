using YoLoTool.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Common.Interfaces;
using OpenCvSharp;

namespace YoLoTool.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class ContentView : INavigableView<ContentViewModel>
    {
        public ContentViewModel ViewModel
        {
            get;
        }

        public ContentView(ContentViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
        }

        public Point2d GetImageCoordsAt(MouseEventArgs e)
        {
            var point = new Point2d(-1, -1);
            if (image != null && image.IsMouseOver)
            {
                var p = e.GetPosition(image);
                double pixelWidth = image.Source.Width;
                double pixelHeight = image.Source.Height;
                double x = pixelWidth * p.X / image.ActualWidth;
                double y = pixelHeight * p.Y / image.ActualHeight;
                point = new Point2d(x, y);
            }

            return point;
        } 

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            var coords = GetImageCoordsAt(e);
            ViewModel.XPosition = coords.X;
            ViewModel.YPosition = coords.Y;
            ViewModel.DrawImage(coords);
            image.Focus();
        }

        private void image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.AddPoints(new Point2d(ViewModel.XPosition, ViewModel.YPosition));
        }

        private void ListView_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.XPosition = 0;
            ViewModel.YPosition = 0;
        }

        private void imagesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            imagesList.ScrollIntoView(imagesList.SelectedItem);
        } 

        private void image_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ViewModel.RemoveSelectedRectAndPoints();
            }
        }
    }   
}

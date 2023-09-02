using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Wpf.Ui.Common.Interfaces;
using YoLoTool.AI.Models;
using YoLoTool.Drawers;
using YoLoTool.Models;

namespace YoLoTool.ViewModels
{
    public partial class ContentViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        public ImagesAttributeContainer imagesAttributeContainer;

        [ObservableProperty]
        public ImageAttributes selectedImage;

        [ObservableProperty]
        public double xPosition;

        [ObservableProperty]
        public double yPosition;

        public ContentViewModel(ImagesAttributeContainer imagesAttributeContainer)
        {
            ImagesAttributeContainer = imagesAttributeContainer;
            if (ImagesAttributeContainer.Images.Any())
            {
                SelectedImage = ImagesAttributeContainer.Images.First();
            } 
        }

        public void DrawImage(Point2d point2d)
        {
            Draw(point2d, SelectedImage);
        }

        public static void Draw(Point2d point2d, ImageAttributes image)
        {
            using var drawMat = image.Mat.Clone();
            var rectsInRoi = new List<Rect>();
            foreach (var rect in image.Rects)
            {
                drawMat.DrawRect(rect);
                if (rect.Contains((int)point2d.X, (int)point2d.Y))
                {
                    rectsInRoi.Add(rect);
                }
            }
            
            if (rectsInRoi.Any())
            {
                var newSelectedRect = rectsInRoi.MinBy(x => (x.Width * x.Height));
                drawMat.DrawRectInside(newSelectedRect);
                image.SelectedRect = newSelectedRect; 
            }
            else
            {
                image.SelectedRect = new();
            }

            foreach (var point in image.Points)
            {
                drawMat.DrawPoint(point);
            }

            drawMat.DrawLines(point2d);
            image.Image = drawMat.ToWriteableBitmap();
        }

        public void AddPoints(Point2d point2d)
        {
            SelectedImage.Points.Add(new Point(point2d.X, point2d.Y));
            if (SelectedImage.Points.Count % 2 == 0)
            {
                var points = SelectedImage.Points.TakeLast(2).ToList();
                SelectedImage.Rects.Add(
                    new Rect(
                        points[0].X,
                        points[0].Y,
                        Math.Abs(points[1].X - points[0].X),
                        Math.Abs(points[1].Y - points[0].Y)));
            }
        }

        [RelayCommand]
        private void DoneImage()
        {
            if (SelectedImage != null)
            {
                SelectedImage.Done = true;
            }    
        }

        [RelayCommand]
        private void PreviousImage()
        {
            if (ImagesAttributeContainer.Images.Any())
            {
                if (ImagesAttributeContainer.Images.IndexOf(SelectedImage) > 0)
                {
                    SelectedImage = ImagesAttributeContainer.Images[ImagesAttributeContainer.Images.IndexOf(SelectedImage) - 1];
                }
            }
        }

        [RelayCommand]
        private void NextImage()
        {
            if (ImagesAttributeContainer.Images.Any())
            {
                if (ImagesAttributeContainer.Images.IndexOf(SelectedImage) < ImagesAttributeContainer.Images.Count - 1)
                {
                    SelectedImage = ImagesAttributeContainer.Images[ImagesAttributeContainer.Images.IndexOf(SelectedImage) + 1];
                }
            }
        }

        internal void RemoveSelectedRectAndPoints()
        {
            if (SelectedImage == null)
            {
                return;
            }
            if (!SelectedImage.Points.Any() || !SelectedImage.Rects.Any())
            {
                return;
            }
            var p1 = SelectedImage.Points.Where(x => x.X == SelectedImage.SelectedRect.X).FirstOrDefault();
            var p2 = SelectedImage.Points.Where(x => x.X == SelectedImage.SelectedRect.X + SelectedImage.SelectedRect.Width).FirstOrDefault();
            if (p1 != null && p2 != null)
            {
                SelectedImage.Points.Remove(p1);
                SelectedImage.Points.Remove(p2);
            } 
            SelectedImage.Rects.Remove(SelectedImage.SelectedRect);
            DrawImage(new Point2d(XPosition, YPosition));
        }

        public void OnNavigatedTo()
        {
            if (SelectedImage == null && ImagesAttributeContainer.Images.Any())
            {
                SelectedImage = ImagesAttributeContainer.Images.First();
            }
            foreach (var image in ImagesAttributeContainer.Images)
            {
                Draw(new Point2d(), image);
            }
        }

        public void OnNavigatedFrom()
        {
        }
    }
}

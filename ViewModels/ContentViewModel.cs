using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using Wpf.Ui.Common.Interfaces;
using YoLoTool.AI;
using YoLoTool.AI.Models;
using YoLoTool.Drawers;
using YoLoTool.Enums;
using YoLoTool.Models;

namespace YoLoTool.ViewModels
{
    public partial class ContentViewModel : ObservableObject, INavigationAware
    {
        private const int ToleranceRectResize = 5;

        private bool _pressedInRect;

        private Point2d _pressedInRectPoint;

        [ObservableProperty]
        public Cursor cursor;

        [ObservableProperty]
        public ImagesAttributeContainer imagesAttributeContainer;

        [ObservableProperty]
        public ImageAttributes selectedImage;

        [ObservableProperty]
        public YoloLabel selectedLabel;

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

        public void SetPressEvent()
        {
            _pressedInRect = false;
            foreach (var rect in SelectedImage.Rects)
            {
                var innerResult = Utils.IsPointInRectBorderWithTolerance(rect, new Point2d(XPosition, YPosition), ToleranceRectResize);
                if (innerResult != Enums.EPointInRectResult.False)
                {
                    _pressedInRect = true;
                    _pressedInRectPoint = new Point2d(XPosition, YPosition);
                }
            }
        }

        public void DrawImage(Point2d point2d, bool pressed = false)
        {
            Draw(point2d, SelectedImage, pressed);
        }

        public void Draw(Point2d point2d, ImageAttributes image, bool pressed = false)
        {
            using var drawMat = image.Mat.Clone();
            var rectsInBoundPoint = new List<Rect>(); 
            var rectsInRoi = new List<Rect>();
            Cursor = Cursors.Cross;
            foreach (var rect in image.Rects)
            {
                drawMat.DrawRect(rect);
                var innerResult = Utils.IsPointInRectBorderWithTolerance(rect, point2d, ToleranceRectResize);
                if (rect.Contains((int)point2d.X, (int)point2d.Y))
                {
                    rectsInRoi.Add(rect);
                }
                if (innerResult != Enums.EPointInRectResult.False)
                {
                    rectsInBoundPoint.Add(rect);
                }
            } 
            if (rectsInRoi.Any())
            {
                var newSelectedRect = rectsInRoi.MinBy(x => (x.Width * x.Height));
                drawMat.DrawSelectedRect(newSelectedRect);
                image.SelectedRect = newSelectedRect;
            }
            else
            {
                image.SelectedRect = new();
            }
            if (rectsInBoundPoint.Any())
            {
                var newSelectedRect = rectsInBoundPoint.MinBy(x => (x.Width * x.Height));
                var result = Utils.IsPointInRectBorderWithTolerance(newSelectedRect, point2d, ToleranceRectResize);
                drawMat.DrawResizingLine(newSelectedRect, result);
                switch (result)
                {
                    case Enums.EPointInRectResult.Left:
                    case Enums.EPointInRectResult.Right:
                        Cursor = Cursors.SizeWE;
                        break;
                    case Enums.EPointInRectResult.Top:
                    case Enums.EPointInRectResult.Bottom:
                        Cursor = Cursors.SizeNS;
                        break;
                } 
                drawMat.DrawResizingLine(newSelectedRect, result);
                if (_pressedInRect && pressed)
                {
                    var x = _pressedInRectPoint.X;
                    var y = _pressedInRectPoint.Y;
                    var width = newSelectedRect.Width;
                    var height = newSelectedRect.Height;
                    var xDiff = x - point2d.X;
                    var yDiff = y - point2d.Y;
                    var index = image.Rects.IndexOf(newSelectedRect);
                    var newRect = newSelectedRect;
                    switch (result)
                    {
                        case EPointInRectResult.Left:
                            newRect = new Rect((int)(x - xDiff), newRect.Y, (int)(width ), (int)height);
                            break;
                        //case EPointInRectResult.Right:
                        //    newRect = image.Rects[index] = new Rect((int)(x), (int)y, (int)(width - xDiff), (int)height);
                        //    break;
                        //case EPointInRectResult.Top:
                        //    newRect = image.Rects[index] = new Rect((int)x, (int)(y - yDiff), (int)width, (int)(height - yDiff));
                        //    break;
                        //case EPointInRectResult.Bottom:
                        //    newRect = image.Rects[index] = new Rect((int)x, (int)y, (int)width, (int)(height - yDiff));
                        //    break;
                    }
                    image.Rects[index] = newRect;
                    image.SelectedRect = newRect;
                }
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
                var rect = new Rect(
                        points[0].X,
                        points[0].Y,
                        Math.Abs(points[1].X - points[0].X),
                        Math.Abs(points[1].Y - points[0].Y));
                if (SelectedLabel != null)
                {
                    SelectedImage.ObjectByRect.Add(rect, SelectedLabel.Name);
                }
                SelectedImage.Points = SelectedImage.Points.Except(points).ToList();
                SelectedImage.Rects.Add(rect);
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
            if (!SelectedImage.Rects.Any())
            {
                return;
            }
            SelectedImage.Rects.Remove(SelectedImage.SelectedRect);
            DrawImage(new Point2d(XPosition, YPosition));
        }

        public void OnNavigatedTo()
        {
            if (ImagesAttributeContainer.Labels.Any())
            {
                SelectedLabel = ImagesAttributeContainer.Labels.First();
            }
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private bool _lock;

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

        public void SetPressEvent(bool isPressed)
        {
            if (!isPressed)
            {
                SelectedImage.SelectedRectIndex = -1;
                Trace.WriteLine("Released");
                _lock = false;
                return;
            }
            if (!_lock)
            {
                _pressedInRect = false; 
                foreach (var rect in SelectedImage.Rects)
                {
                    var innerResult = Utils.IsPointInRectBorderWithTolerance(rect, new Point2d(XPosition, YPosition), ToleranceRectResize);
                    if (innerResult != EPointInRectResult.False)
                    {
                        SelectedImage.SelectedRectIndex = SelectedImage.Rects.IndexOf(rect);
                        Trace.WriteLine("Rect changed");
                        _pressedInRect = true;
                        _pressedInRectPoint = new Point2d(XPosition, YPosition);
                    }
                }
                _lock = true;
                Trace.WriteLine("Locked");
            }
        }

        public void DrawImage(Point2d point2d, bool pressed = false)
        {
            DrawAll(point2d, SelectedImage, pressed);
        }

        public void DrawAll(Point2d point2d, ImageAttributes image, bool pressed = false)
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
                if (innerResult != EPointInRectResult.False)
                {
                    rectsInBoundPoint.Add(rect);
                }
            }
            var commonRects = rectsInRoi.Intersect(rectsInBoundPoint).ToList();
            if (commonRects.Any())
            {
                var newSelectedRect = commonRects.MinBy(x => (x.Width * x.Height));
                drawMat.DrawSelectedRect(newSelectedRect);
                image.SelectedRect = newSelectedRect;
                var rectToDrawLine = newSelectedRect;
                var result = Utils.IsPointInRectBorderWithTolerance(rectToDrawLine, point2d, ToleranceRectResize);
                drawMat.DrawResizingLine(rectToDrawLine, result);
                switch (result)
                {
                    case EPointInRectResult.Left:
                    case EPointInRectResult.Right:
                        Cursor = Cursors.SizeWE;
                        break;
                    case EPointInRectResult.Top:
                    case EPointInRectResult.Bottom:
                        Cursor = Cursors.SizeNS;
                        break;
                }
            }
            else
            {
                image.SelectedRect = new();
            }
            if (rectsInBoundPoint.Any())
            {
                var rectToDrawLine = rectsInBoundPoint.MinBy(x => (x.Width * x.Height));
                var result = Utils.IsPointInRectBorderWithTolerance(rectToDrawLine, point2d, ToleranceRectResize);
                drawMat.DrawResizingLine(rectToDrawLine, result);
                switch (result)
                {
                    case EPointInRectResult.Left:
                    case EPointInRectResult.Right:
                        Cursor = Cursors.SizeWE;
                        break;
                    case EPointInRectResult.Top:
                    case EPointInRectResult.Bottom:
                        Cursor = Cursors.SizeNS;
                        break;
                }
                var newSelectedRect = new Rect();
                if (SelectedImage.SelectedRectIndex != -1)
                {
                    newSelectedRect = SelectedImage.Rects[SelectedImage.SelectedRectIndex];
                }
                else
                {
                    newSelectedRect = rectToDrawLine;
                }
                if (_pressedInRect && pressed)
                {
                    var index = image.Rects.IndexOf(newSelectedRect);
                    var newRect = newSelectedRect;
                    var x = _pressedInRectPoint.X;
                    var y = _pressedInRectPoint.Y;
                    var width = newSelectedRect.Width;
                    var height = newSelectedRect.Height;
                    var xDiff = x - point2d.X;
                    var yDiff = y - point2d.Y;
                    if (result == EPointInRectResult.Right)
                    {
                        xDiff += newRect.Width;
                    }
                    if (result == EPointInRectResult.Bottom)
                    {
                        yDiff += newRect.Height;
                    }
                    var newX = (int)(x - xDiff);
                    var newY = (int)(y - yDiff);
                    var widthDiff = newX - newRect.X;
                    var heightDiff = newY - newRect.Y;
                    var newWidth = newRect.Width - widthDiff;
                    var newHeight = newRect.Height - heightDiff;
                    if (result == EPointInRectResult.Right)
                    {
                        newWidth += (int)(2 * widthDiff);
                    }
                    if (result == EPointInRectResult.Bottom)
                    {
                        newHeight += (int)(2 * heightDiff);
                    }
                    switch (result)
                    {
                        case EPointInRectResult.Left:
                            newRect = new Rect(newX, newRect.Y, newWidth, newRect.Height);
                            break;
                        case EPointInRectResult.Right:
                            newRect = new Rect(newRect.X, newRect.Y, newWidth, newRect.Height);
                            break;
                        case EPointInRectResult.Top:
                            newRect = new Rect(newRect.X, newY, newRect.Width, newHeight);
                            break;
                        case EPointInRectResult.Bottom:
                            newRect = new Rect(newRect.X, newRect.Y, newRect.Width, newHeight);
                            break;
                    }
                    image.Rects[index] = newRect;
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
                float top = Math.Min(points[0].Y, points[1].Y);
                float bottom = Math.Max(points[0].Y, points[1].Y);
                float left = Math.Min(points[0].X, points[1].X);
                float right = Math.Max(points[0].X, points[1].X);
                var rect = new Rect((int)left, (int)top, (int)(right - left), (int)(bottom - top));
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
                DrawAll(new Point2d(), image);
            }
        }

        public void OnNavigatedFrom()
        {
        }
    }
}

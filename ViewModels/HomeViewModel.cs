using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wpf.Ui.Mvvm.Contracts;
using YoLoTool.AI.Interfaces;
using YoLoTool.AI.Models;
using YoLoTool.Models;
using YoLoTool.Services;
using YoLoTool.Views;

namespace YoLoTool.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private const string OnnxModelExtension = "onnx";

        private const int DelayAfterAllStages = 1000;

        private readonly LoaderService _loaderService;

        private readonly ISnackbarService _snackbarService;

        private readonly INavigationService _navigationService;

        private readonly ImagesAttributeContainer _imagesAttributeContainer;

        private readonly IYolo _yolo;

        private List<string> _imagesPaths;

        private readonly List<string> _allowedLabelNames;

        [ObservableProperty]
        public string dataPath;

        [ObservableProperty]
        public string modelPath;

        [ObservableProperty]
        public int totalDataCount;

        [ObservableProperty]
        public int processedDataCount;

        [ObservableProperty]
        public bool stage1Completed;

        [ObservableProperty]
        public bool stage2Completed;

        [ObservableProperty]
        public bool stage3Completed;

        [ObservableProperty]
        public bool isSpinnerVisible;

        public HomeViewModel( IYolo yolo,
            LoaderService loaderService,
            ImagesAttributeContainer imagesAttributeContainer,
            ISnackbarService snackbarService,
            INavigationService navigationService)
        {
            _navigationService = navigationService;
            _loaderService = loaderService;
            _yolo = yolo;
            _snackbarService = snackbarService;
            _imagesAttributeContainer = imagesAttributeContainer;
            _imagesPaths = new ();
            _allowedLabelNames = new();
        }

        [RelayCommand]
        private void LoadData()
        {
            var path = _loaderService.GetSingleFolderPath();
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
            var files = GetFilesFrom(path, filters, false).ToList();
            if (files.Any())
            {
                _imagesPaths = files;
                Stage2Completed = true;
                DataPath = System.IO.Path.GetFileName(path);
                TotalDataCount = files.Count();
                _snackbarService.Show("Success", "Data was successfully loaded", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
            }
        }
        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            var filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        [RelayCommand]
        private void LoadModel()
        {
            var path = _loaderService.GetSingleFilePath(OnnxModelExtension);
            if (_yolo.LoadYoloModel(path))
            {
                ModelPath = System.IO.Path.GetFileName(path);
                _snackbarService.Show("Success", "Yolo model was successfully loaded", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
                Stage1Completed = true;
            }
        }

        [RelayCommand]
        private async void RunPrelabel()
        {
            var counter = 0;
            var prelabeledCounter = 0;
            IsSpinnerVisible = true;
            foreach (var path in _imagesPaths)
            {
                var image = new ImageAttributes();
                image.LoadImage(path);
                var successFlag = false;
                await Task.Run(() =>
                {
                    foreach (var detection in _yolo.Predict(image.Mat))
                    {
                        if (detection.Label == null)
                        {
                            continue;
                        }
                        if (_allowedLabelNames.Contains(detection.Label.Name.ToLowerInvariant()) || !_allowedLabelNames.Any())
                        {
                            var rectangle = detection.Rectangle;
                            var rect = _yolo.CastToOriginalSize(
                                image.Mat.Size(), 
                                new OpenCvSharp.Rect(
                                    (int)rectangle.X, 
                                    (int)rectangle.Y,
                                    (int)rectangle.Width, 
                                    (int)rectangle.Height));
                            image.Rects.Add(new OpenCvSharp.Rect(
                                (int)rect.X,
                                (int)rect.Y,
                                (int)rect.Width,
                                (int)rect.Height));
                            image.Points.Add(new OpenCvSharp.Point(rect.X, rect.Y));
                            image.Points.Add(new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height));
                            successFlag = true;
                        }
                    }
                    if (successFlag)
                    {
                        prelabeledCounter++;
                    }
                });
                counter++;
                await App.Current.Dispatcher.BeginInvoke(() =>
                { 
                    ProcessedDataCount = counter;
                });
                _imagesAttributeContainer.Images.Add(image);
            }
            IsSpinnerVisible = false;
            Stage3Completed = true;
            _snackbarService.Show("Success", $"Successfully prelabeled {prelabeledCounter} images.", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
            await Task.Delay(DelayAfterAllStages);
            _navigationService.Navigate(typeof(ContentView));
        }
    }
}
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
using YoLoTool.Enums;
using YoLoTool.Extensions;
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

        private readonly IYolo _yolo;

        private List<string> _imagesPaths;

        private readonly List<string> _allowedLabelNames;

        [ObservableProperty]
        private ImagesAttributeContainer imagesAttributeContainer;

        [ObservableProperty]
        public string dataPath;

        [ObservableProperty]
        public string modelPath;

        [ObservableProperty]
        public string newLabelText = "";

        [ObservableProperty]
        public int removeLabelIndex;

        [ObservableProperty]
        public int totalDataCount;

        [ObservableProperty]
        public int processedDataCount;

        [ObservableProperty]
        public ERunMode runMode;

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
            this.imagesAttributeContainer = imagesAttributeContainer;
            _imagesPaths = new ();
            _allowedLabelNames = new(); 
#if DEBUG
            _yolo.LoadYoloModel("AI\\Yolo\\yolov7-tiny.onnx");
            Stage1Completed = true;
#endif
        }

        partial void OnRunModeChanged(ERunMode value)
        {
            Stage1Completed = value == ERunMode.Labeling;
        }

        [RelayCommand]
        private async Task LoadData()
        {
            var path = _loaderService.GetSingleFolderPath();
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
            var files = Helpers.IOHelper.GetFilesFrom(path, filters, false).ToList();
            if (files.Any())
            {
                _imagesPaths = files;
                Stage2Completed = true;
                DataPath = Path.GetFileName(path);
                TotalDataCount = files.Count;
                _snackbarService.Show("Success", "Data was successfully loaded", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
                if (RunMode == ERunMode.Labeling)
                {
                    foreach (var imagePath in _imagesPaths)
                    {
                        var image = new ImageAttributes();
                        image.LoadImage(imagePath);
                        lock (imagesAttributeContainer.Locker)
                        {
                            imagesAttributeContainer.Images.Add(image);
                        }
                    }
                    await Task.Delay(DelayAfterAllStages);
                    _navigationService.Navigate(typeof(ContentView));
                }
            }
        }

        [RelayCommand]
        private void LoadModel()
        {
            var path = _loaderService.GetSingleFilePath(OnnxModelExtension);
            if (_yolo.LoadYoloModel(path))
            {
                ModelPath = Path.GetFileName(path);
                ImagesAttributeContainer.Labels = new(_yolo.GetModelLabels());
                _snackbarService.Show("Success", "Yolo model was successfully loaded", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
                Stage1Completed = true;
            }
        }

        [RelayCommand]
        private async Task RunPrelabel()
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
                            var imageRect = new OpenCvSharp.Rect(
                                (int)rect.X,
                                (int)rect.Y,
                                (int)rect.Width,
                                (int)rect.Height);
                            image.Rects.Add(imageRect);
                            image.ObjectByRect.Add(imageRect, detection.Label.Name);
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
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                { 
                    ProcessedDataCount = counter;
                });
                lock (imagesAttributeContainer.Locker)
                {
                    imagesAttributeContainer.Images.Add(image);
                }
            }
            IsSpinnerVisible = false;
            Stage3Completed = true;
            _snackbarService.Show("Success", $"Successfully prelabeled {prelabeledCounter} images.", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
            await Task.Delay(DelayAfterAllStages);
            _navigationService.Navigate(typeof(ContentView));
        }

        [RelayCommand]
        private void AddLabel()
        {
            if (string.IsNullOrEmpty(NewLabelText) || NewLabelText.Count() < 3)
            {
                _snackbarService.Show("Fail", "New label should have at least 3 signs.", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Danger);
                return;
            }
            ImagesAttributeContainer.Labels.Add(new YoloLabel()
            {
                Id = ImagesAttributeContainer.Labels.Count,
                Name = NewLabelText,
                Color = NewLabelText.GetLabelColor()
            }); 
            _snackbarService.Show("Success", $"Successfully added new label: Name: {NewLabelText} ID: {ImagesAttributeContainer.Labels.Count-1}.", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);
            NewLabelText = "";
        }

        [RelayCommand]
        private void RemoveLabel()
        {
            if (!ImagesAttributeContainer.Labels.Any())
            {
                _snackbarService.Show("Fail", "There is no labels, nothing to remove.", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Danger);
                return;
            }
            var itemToRemove = ImagesAttributeContainer.Labels.FirstOrDefault(x => x.Id == RemoveLabelIndex);
            if (itemToRemove == null)
            {
                _snackbarService.Show("Fail", $"Label with given ID ({RemoveLabelIndex}) was not found.", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Danger);
                return;
            }
            ImagesAttributeContainer.Labels.Remove(itemToRemove); 
            _snackbarService.Show("Success", $"Successfully removed label: Name: {itemToRemove.Name} ID: {itemToRemove.Id}.", Wpf.Ui.Common.SymbolRegular.CheckmarkCircle20, Wpf.Ui.Common.ControlAppearance.Success);

            RemoveLabelIndex = 0;
        }

        [RelayCommand]
        private void Prelabeling()
        {
            RunMode = ERunMode.Prelabeling;
        }

        [RelayCommand]
        private void Labeling()
        {
            RunMode = ERunMode.Labeling;
        }

        [RelayCommand]
        private void Back()
        {
            RunMode = ERunMode.SelectingMode;
        } 
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using YoLoTool.Models;
using YoLoTool.Services;

namespace YoLoTool.ViewModels
{
    public partial class ExportViewModel : ObservableObject
    {
        private readonly LoaderService _loaderService;


        private readonly ImagesAttributeContainer _imagesAttributeContainer;

        public ExportViewModel(ImagesAttributeContainer imagesAttributeContainer,
            LoaderService loaderService)
        {
            _imagesAttributeContainer = imagesAttributeContainer;
            _loaderService = loaderService;
        }

        [RelayCommand]
        private void Export()
        {
            var path = _loaderService.GetSingleSaveFilePath();
            var stringToSave = new StringBuilder();
            foreach (var item in _imagesAttributeContainer.Images)
            {
                var str = new StringBuilder();
                foreach (var rect in item.Rects)
                { 
                    str.Append(" ");
                    str.Append(item.ObjectByRect[rect]);
                    str.Append(" ");
                    str.Append(rect.X);
                    str.Append(" ");
                    str.Append(rect.Y);
                    str.Append(" ");
                    str.Append(rect.Width);
                    str.Append(" ");
                    str.Append(rect.Height);
                    str.Append("\n");
                }
                stringToSave.Append(str.ToString());
            }
            File.WriteAllText(path, stringToSave.ToString());
        }
    }
}

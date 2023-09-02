using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;

namespace YoLoTool.Services
{
    public class LoaderService
    {
        public string GetSingleFilePath(string extension = "")
        { 
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select single file",
                DefaultExt = extension,
                Filter = $"file(s) | *.{extension}" ,
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames.Count() > 0)
                return openFileDialog.FileNames.ToList().First();
            else
                return "";
        }

        public string GetSingleFolderPath()
        {
            var dialog = new CommonOpenFileDialog(); 
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            return "";
        }
    }
}

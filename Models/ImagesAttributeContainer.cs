using CommunityToolkit.Mvvm.ComponentModel;  
using System.Collections.ObjectModel;
using System.Windows.Data;
using YoLoTool.AI.Models;

namespace YoLoTool.Models
{
    public partial class ImagesAttributeContainer : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<ImageAttributes> images;

        public object Locker { get; set; }

        [ObservableProperty]
        public ObservableCollection<YoloLabel> labels;

        public ImagesAttributeContainer()
        {
            Images = new ();
            Locker = new object();
            BindingOperations.EnableCollectionSynchronization(Images, Locker);
        }
    }
}

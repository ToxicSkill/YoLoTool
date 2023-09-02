using CommunityToolkit.Mvvm.ComponentModel; 
using System.Collections.Generic;
using YoLoTool.AI.Models;

namespace YoLoTool.Models
{
    public partial class ImagesAttributeContainer : ObservableObject
    {
        [ObservableProperty]
        public List<ImageAttributes> images;

        public ImagesAttributeContainer()
        {
            Images = new ();
        }
    }
}

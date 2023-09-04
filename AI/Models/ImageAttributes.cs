using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace YoLoTool.AI.Models
{
    public partial class ImageAttributes : ObservableObject
    {
        private Rect _selecedRect;

        public Mat Mat { get; set; }

        [ObservableProperty]
        public WriteableBitmap image;

        public List<Rect> Rects { get; set; }

        public List<Point> Points { get; set; }

        public int SelectedRectIndex { get; set; }

        public Rect SelectedRect 
        {
            get => _selecedRect;
            set 
            {  
                _selecedRect = value;
                if (ObjectByRect.ContainsKey(value))
                {
                    SelectedObject = ObjectByRect[value];
                }
                else
                {
                    SelectedObject = "";
                }
            }
        }

        [ObservableProperty]
        public string selectedObject;

        public Dictionary<Rect, string> ObjectByRect{ get; set; }

        [ObservableProperty]
        public bool done;

        public ImageAttributes()
        {
            Mat = new();
            ObjectByRect = new() { { new Rect(), "" } };
            Rects = new List<Rect>();
            Points = new List<Point>();
        }

        internal void LoadImage(string path)
        {
            var mat = Cv2.ImRead(path);
            Mat = mat;
            Image = Mat.ToWriteableBitmap();
        }
    }
}

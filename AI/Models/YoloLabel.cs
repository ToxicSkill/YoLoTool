using System.Drawing;

namespace YoLoTool.AI.Models
{
    public class YoloLabel
    {
        private string _name = "";

        public int Id { get; set; }

        public string? Name 
        { 
            get => _name;
            set
            {
                if (_name == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _name = value;
                Width = _name!.Length * 10;
            }
        }

        public YoloLabelKind Kind { get; set; }
        
        public int Width { get; set; }

        public Color Color { get; set; }

        public YoloLabel() => Color = Color.Yellow;
    }
}

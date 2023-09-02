using System;
using System.Collections.Generic;
using System.Drawing;

namespace YoLoTool.AI.Models
{
    public class YoloPrediction : IComparable, IComparer<YoloPrediction>
    {
        public YoloLabel? Label { get; set; }

        public RectangleF Rectangle { get; set; }

        public float Score { get; set; }

        public Point Center
        {
            get
            {
                if (!Rectangle.IsEmpty)
                {
                    return new Point((int)(Rectangle.X + Rectangle.Width / 2),
                        (int)(Rectangle.Y + Rectangle.Height / 2));
                }
                else
                {
                    return new Point(0, 0);
                }
            }
        }

        public YoloPrediction() { }

        public YoloPrediction(YoloLabel label, float confidence) : this(label)
        {
            Score = confidence;
        }

        public YoloPrediction(YoloLabel label)
        {
            Label = label;
        }

        public int Compare(YoloPrediction x, YoloPrediction y)
        {
            if (x.Score == y.Score)
                return 0;

            return x.Score.CompareTo(y.Score);
        }

        public int CompareTo(object obj)
        {
            var y = obj as YoloPrediction;
            return Score.CompareTo(y.Score);
        }
    }
}

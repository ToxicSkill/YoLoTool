using OpenCvSharp;
using System.Collections.Generic;
using YoLoTool.AI.Models;

namespace YoLoTool.AI.Interfaces
{
    public interface IYolo
    {
        List<YoloLabel> GetModelLabels();

        Rect CastToOriginalSize(OpenCvSharp.Size size, Rect detectionRect, bool roi = true);

        public bool LoadYoloModel(string path);

        public List<YoloPrediction> Predict(Mat mat);
    }
}

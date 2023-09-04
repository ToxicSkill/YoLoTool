using Microsoft.ML.OnnxRuntime;
using System.Linq;
using YoLoTool.AI.Models;
using YoLoTool.Extensions;

namespace YoLoTool.AI
{
    public class YoloDecorator
    {  
        public void DecorateModelInformations(YoloModel yoloModel, InferenceSession inferenceSession)
        {
            SetupYoloDefaultLabels(yoloModel);
            SetInputDetails(yoloModel, inferenceSession);
            SetOutputDetails(yoloModel, inferenceSession);
        }

        private void SetupLabels(YoloModel yoloModel, string[] labels)
        {
            labels.Select((s, i) => new { i, s }).ToList().ForEach(item =>
            {
                yoloModel.Labels.Add(new YoloLabel() { Id = item.i, Name = item.s, Color = item.s.GetLabelColor() });
            });
        }

        private void SetupYoloDefaultLabels(YoloModel yoloModel)
        {
            var labels = new string[] { "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
            SetupLabels(yoloModel, labels);
        }

        private static void SetInputDetails(YoloModel yoloModel, InferenceSession inferenceSession)
        {
            yoloModel.Height = inferenceSession.InputMetadata["images"].Dimensions[2];
            yoloModel.Width = inferenceSession.InputMetadata["images"].Dimensions[3];
        }

        private static void SetOutputDetails(YoloModel yoloModel, InferenceSession inferenceSession)
        {
            yoloModel.Outputs = inferenceSession.OutputMetadata.Keys.ToArray();
            yoloModel.Dimensions = inferenceSession.OutputMetadata[yoloModel.Outputs[0]].Dimensions[1];
            yoloModel.UseDetect = !yoloModel.Outputs.Any(x => x == "score");
        }
    }
}

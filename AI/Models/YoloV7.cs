using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Concurrent;
using System.Drawing;
using System;
using System.Collections.Generic;
using YoLoTool.AI.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace YoLoTool.AI.Models
{
    public class Yolov7 : IYolo, IDisposable
    {
        private static readonly OpenCvSharp.Size InferenceSize = new OpenCvSharp.Size(640, 640);

        private InferenceSession _inferenceSession;

        private readonly YoloModel _model = new();

        private readonly YoloDecorator _yoloDecorator;

        public Yolov7()
        {
            _yoloDecorator = new YoloDecorator();
        }

        public List<YoloLabel> GetModelLabels()
        {
            return _model.Labels;
        }

        public bool LoadYoloModel(string path)
        {
            try
            {
                var opts = new SessionOptions();
                _inferenceSession = new InferenceSession(path, opts);
                _yoloDecorator.DecorateModelInformations(_model, _inferenceSession);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public List<YoloPrediction> Predict(Mat mat)
        {
            using var img = mat.Clone();
            var x = 0;
            var y = 0;
            if (img.Height > img.Width)
            {
                y = (img.Height - img.Width) / 2;
            }
            else if (img.Height < img.Width)
            {
                x = (img.Width - img.Height) / 2;
            }
            var cutSize = Math.Min(img.Width, img.Height);
            using var cutImg = new Mat(img, new Rect(x, y, cutSize, cutSize));
            Cv2.Resize(cutImg, cutImg, InferenceSize);
            using var image = cutImg.ToBitmap(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            return ParseDetect(Inference(image)[0], image);
        }

        public Rect CastToOriginalSize(OpenCvSharp.Size size, Rect detectionRect, bool roi = true)
        {
            var x = 0;
            var y = 0;
            if (size.Height > size.Width)
            {
                y = (size.Height - size.Width) / 2;
            }
            else if (size.Height < size.Width)
            {
                x = (size.Width - size.Height) / 2;
            }
            var cutSize = Math.Min(size.Width, size.Height);
            var ratioScale = (cutSize / (double)InferenceSize.Width);
            var rectX = detectionRect.X * ratioScale;
            var rectY = detectionRect.Y * ratioScale;
            var width = detectionRect.Width * ratioScale;
            var height = detectionRect.Height * ratioScale;
            if (roi)
            {
                rectX += x;
                rectY += y;
            }
            return new Rect((int)rectX, (int)rectY, (int)width, (int)height);
        }

        private List<YoloPrediction> ParseDetect(DenseTensor<float> output, Image image)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (image.Width, image.Height);
            var (xGain, yGain) = (_model.Width / (float)w, _model.Height / (float)h);
            var gain = Math.Min(xGain, yGain);

            var (xPad, yPad) = ((_model.Width - w * gain) / 2, (_model.Height - h * gain) / 2);

            Parallel.For(0, output.Dimensions[0], i =>
            {
                var span = output.Buffer.Span.Slice(i * output.Strides[0]);
                var label = _model.Labels[(int)span[5]];
                var prediction = new YoloPrediction(label, span[6]);

                var xMin = (span[1] - xPad) / gain;
                var yMin = (span[2] - yPad) / gain;
                var xMax = (span[3] - xPad) / gain;
                var yMax = (span[4] - yPad) / gain;

                prediction.Rectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
                result.Add(prediction);
            });

            return result.ToList();
        }

        private DenseTensor<float>[] Inference(Image img)
        {
            var bmp = new Bitmap(img);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", Utils.ExtractPixelsFast(bmp))
            };

            var result = _inferenceSession.Run(inputs);

            var output = new List<DenseTensor<float>>();

            foreach (var item in _model.Outputs)
            {
                output.Add((DenseTensor<float>)result.First(x => x.Name == item).Value);
            };

            return output.ToArray();
        }

        public void Dispose()
        {
            _inferenceSession.Dispose();
        }
    }
}
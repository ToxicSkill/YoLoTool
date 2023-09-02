﻿using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace YoLoTool.AI
{
    public static class Utils
    {
        private const float Dividor = 255.0f;

        public static Tensor<float> ExtractPixelsFast(Bitmap bitmap)
        {
            var pixelCount = bitmap.Width * bitmap.Height;
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var tensor = new DenseTensor<float>(new[] { 1, 3, bitmap.Height, bitmap.Width });
            Span<byte> data;

            BitmapData bitmapData;
            bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                data = new Span<byte>((void*)bitmapData.Scan0, bitmapData.Height * bitmapData.Stride);
            }

            ExtractPixelsRgb(tensor, data, pixelCount);

            bitmap.UnlockBits(bitmapData);

            return tensor;
        }

        public static void ExtractPixelsRgb(DenseTensor<float> tensor, Span<byte> data, int pixelCount)
        {
            var spanR = tensor.Buffer.Span;
            var spanG = spanR[pixelCount..];
            var spanB = spanG[pixelCount..];

            var sidx = 0;
            var didx = 0;
            for (var i = 0; i < pixelCount; i++)
            {
                spanR[didx] = data[sidx + 2] / Dividor;
                spanG[didx] = data[sidx + 1] / Dividor;
                spanB[didx] = data[sidx] / Dividor;
                didx++;
                sidx += 3;
            }
        }
    }
}
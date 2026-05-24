using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PixelLab.Models;
using PixelLab.Enums;
using PixelLab.Utils;

namespace PixelLab.Servicess
{
    public class ImageColorDistribution3DService
    {
        public ImageColorDistribution3DResult BuildDistribution(
            Bitmap sourceBitmap,
            ImageColorDistribution3DSettings settings)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Stopwatch stopwatch = Stopwatch.StartNew();

            ImageColorDistribution3DResult result = CreateEmptyResult(settings.ProjectionType);

            using (Bitmap source24 = Helpers.Ensure24bppRgb(sourceBitmap))
            {
                int width = source24.Width;
                int height = source24.Height;

                result.OriginalPixelCount = width * height;

                int step = CalculateSamplingStep(width, height, settings.MaxSampleCount);

                Rectangle rect = new Rectangle(0, 0, width, height);

                BitmapData data = source24.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                try
                {
                    int byteCount = Math.Abs(data.Stride) * height;
                    byte[] bytes = new byte[byteCount];

                    Marshal.Copy(data.Scan0, bytes, 0, byteCount);

                    for (int y = 0; y < height; y += step)
                    {
                        int row = y * data.Stride;

                        for (int x = 0; x < width; x += step)
                        {
                            int index = row + x * 3;

                            byte b = bytes[index + 0];
                            byte g = bytes[index + 1];
                            byte r = bytes[index + 2];

                            Color displayColor = Color.FromArgb(r, g, b);

                            ImageColorPoint3D point = ProjectColor(r, g, b, displayColor, settings.ProjectionType);

                            result.Points.Add(point);
                        }
                    }
                }
                finally
                {
                    source24.UnlockBits(data);
                }
            }

            result.SampledPointCount = result.Points.Count;

            stopwatch.Stop();
            result.ProcessingMilliseconds = stopwatch.ElapsedMilliseconds;

            return result;
        }

        private ImageColorDistribution3DResult CreateEmptyResult(ImageColorProjection3DType projection)
        {
            ImageColorDistribution3DResult result = new ImageColorDistribution3DResult();

            result.ProjectionType = projection;

            switch (projection)
            {
                case ImageColorProjection3DType.RgbCube:
                    result.Title = "3D Image Color Distribution - RGB Cube";
                    result.XAxisLabel = "R";
                    result.YAxisLabel = "G";
                    result.ZAxisLabel = "B";
                    break;

                case ImageColorProjection3DType.HsvCylinder:
                    result.Title = "3D Image Color Distribution - HSV Cylinder";
                    result.XAxisLabel = "Hue X";
                    result.YAxisLabel = "Hue Y";
                    result.ZAxisLabel = "Value";
                    break;

                case ImageColorProjection3DType.LabSpace:
                    result.Title = "3D Image Color Distribution - LAB Space";
                    result.XAxisLabel = "a";
                    result.YAxisLabel = "b";
                    result.ZAxisLabel = "L";
                    break;

                case ImageColorProjection3DType.YCbCrSpace:
                    result.Title = "3D Image Color Distribution - YCbCr Space";
                    result.XAxisLabel = "Cb";
                    result.YAxisLabel = "Cr";
                    result.ZAxisLabel = "Y";
                    break;

                case ImageColorProjection3DType.YuvSpace:
                    result.Title = "3D Image Color Distribution - YUV Space";
                    result.XAxisLabel = "U";
                    result.YAxisLabel = "V";
                    result.ZAxisLabel = "Y";
                    break;

                case ImageColorProjection3DType.CmykCmkSpace:
                    result.Title = "3D Image Color Distribution - CMYK C-M-K Space";
                    result.XAxisLabel = "C";
                    result.YAxisLabel = "M";
                    result.ZAxisLabel = "K";
                    break;
            }

            return result;
        }

        private ImageColorPoint3D ProjectColor(
            byte r, byte g, byte b, Color displayColor, ImageColorProjection3DType projection
        )
        {
            if (projection == ImageColorProjection3DType.RgbCube)
            {
                float x = Normalize255ToMinusPlus(r);
                float y = Normalize255ToMinusPlus(g);
                float z = Normalize255ToMinusPlus(b);

                string text = "RGB point: R=" + r + ", G=" + g + ", B=" + b;

                return new ImageColorPoint3D(x, y, z, displayColor, text);
            }

            if (projection == ImageColorProjection3DType.HsvCylinder)
            {
                double h, s, v;

                ColorValueConversions.RgbToHsv(r, g, b, out h, out s, out v);

                double radians = h * Math.PI / 180.0;

                float x = (float)(Math.Cos(radians) * s);
                float y = (float)(Math.Sin(radians) * s);
                float z = (float)(v * 2.0 - 1.0);

                string text =
                    "HSV point: H=" + h.ToString("0.0") +
                    "°, S=" + (s * 100.0).ToString("0.0") +
                    "%, V=" + (v * 100.0).ToString("0.0") + "%";

                return new ImageColorPoint3D(x, y, z, displayColor, text);
            }

            if (projection == ImageColorProjection3DType.LabSpace)
            {
                double l, a, labB;

                ColorValueConversions.RgbToLab(r, g, b, out l, out a, out labB);

                float x = (float)(Clamp01((a + 128.0) / 255.0) * 2.0 - 1.0);
                float y = (float)(Clamp01((labB + 128.0) / 255.0) * 2.0 - 1.0);
                float z = (float)(Clamp01(l / 100.0) * 2.0 - 1.0);

                string text =
                    "LAB point: L=" + l.ToString("0.0") +
                    ", a=" + a.ToString("0.0") +
                    ", b=" + labB.ToString("0.0");

                return new ImageColorPoint3D(x, y, z, displayColor, text);
            }

            if (projection == ImageColorProjection3DType.YCbCrSpace)
            {
                double yValue, cb, cr;

                ColorValueConversions.RgbToYCbCr(r, g, b, out yValue, out cb, out cr);

                float x = Normalize255ToMinusPlus(cb);
                float yy = Normalize255ToMinusPlus(cr);
                float z = Normalize255ToMinusPlus(yValue);

                string text =
                    "YCbCr point: Y=" + yValue.ToString("0.0") +
                    ", Cb=" + cb.ToString("0.0") +
                    ", Cr=" + cr.ToString("0.0");

                return new ImageColorPoint3D(x, yy, z, displayColor, text);
            }

            if (projection == ImageColorProjection3DType.YuvSpace)
            {
                double yValue, u, v;

                ColorValueConversions.RgbToYuv(r, g, b, out yValue, out u, out v);

                float x = Normalize255ToMinusPlus(u);
                float yy = Normalize255ToMinusPlus(v);
                float z = Normalize255ToMinusPlus(yValue);

                string text =
                    "YUV point: Y=" + yValue.ToString("0.0") +
                    ", U=" + u.ToString("0.0") +
                    ", V=" + v.ToString("0.0");

                return new ImageColorPoint3D(x, yy, z, displayColor, text);
            }

            if (projection == ImageColorProjection3DType.CmykCmkSpace)
            {
                double c, m, yy, k;

                ColorValueConversions.RgbToCmyk(r, g, b, out c, out m, out yy, out k);

                float x = (float)(c * 2.0 - 1.0);
                float y = (float)(m * 2.0 - 1.0);
                float z = (float)(k * 2.0 - 1.0);

                string text =
                    "CMYK point: C=" + (c * 100.0).ToString("0.0") +
                    "%, M=" + (m * 100.0).ToString("0.0") +
                    "%, Y=" + (yy * 100.0).ToString("0.0") +
                    "%, K=" + (k * 100.0).ToString("0.0") + "%";

                return new ImageColorPoint3D(x, y, z, displayColor, text);
            }

            return new ImageColorPoint3D(0, 0, 0, displayColor, "");
        }

        private int CalculateSamplingStep(int width, int height, int maxSampleCount)
        {
            if (maxSampleCount <= 0)
                return 1;

            double totalPixels = width * height;

            if (totalPixels <= maxSampleCount)
                return 1;

            double ratio = totalPixels / maxSampleCount;

            int step = (int)Math.Ceiling(Math.Sqrt(ratio));

            if (step < 1)
                step = 1;

            return step;
        }

        private float Normalize255ToMinusPlus(double value)
        {
            value = Clamp255(value);

            return (float)(value / 255.0 * 2.0 - 1.0);
        }

        private double Clamp01(double value)
        {
            if (value < 0)
                return 0;

            if (value > 1)
                return 1;

            return value;
        }

        private double Clamp255(double value)
        {
            if (value < 0)
                return 0;

            if (value > 255)
                return 255;

            return value;
        }
    }
}
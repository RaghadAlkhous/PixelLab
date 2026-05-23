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
    public class ImageColorDistribution2DService
    {
        public ImageColorDistribution2DResult BuildDistribution(
            Bitmap sourceBitmap, ImageColorDistribution2DSettings settings
        )
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Stopwatch stopwatch = Stopwatch.StartNew();

            ImageColorDistribution2DResult result = CreateEmptyResult(settings.ProjectionType);

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

                            ImageColorPoint2D point = ProjectColor(r, g, b, displayColor, settings.ProjectionType);

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

        private ImageColorDistribution2DResult CreateEmptyResult(ImageColorProjection2DType projection)
        {
            ImageColorDistribution2DResult result = new ImageColorDistribution2DResult();

            switch (projection)
            {
                case ImageColorProjection2DType.Rgb_RG:
                    result.Title = "Image Colors in RGB: R-G Projection";
                    result.XAxisLabel = "R";
                    result.YAxisLabel = "G";
                    break;

                case ImageColorProjection2DType.Rgb_RB:
                    result.Title = "Image Colors in RGB: R-B Projection";
                    result.XAxisLabel = "R";
                    result.YAxisLabel = "B";
                    break;

                case ImageColorProjection2DType.Rgb_GB:
                    result.Title = "Image Colors in RGB: G-B Projection";
                    result.XAxisLabel = "G";
                    result.YAxisLabel = "B";
                    break;

                case ImageColorProjection2DType.Hsv_HS:
                    result.Title = "Image Colors in HSV: H-S Projection";
                    result.XAxisLabel = "Hue";
                    result.YAxisLabel = "Saturation";
                    break;

                case ImageColorProjection2DType.Hsv_HV:
                    result.Title = "Image Colors in HSV: H-V Projection";
                    result.XAxisLabel = "Hue";
                    result.YAxisLabel = "Value";
                    break;

                case ImageColorProjection2DType.Hsv_SV:
                    result.Title = "Image Colors in HSV: S-V Projection";
                    result.XAxisLabel = "Saturation";
                    result.YAxisLabel = "Value";
                    break;

                case ImageColorProjection2DType.Lab_AB:
                    result.Title = "Image Colors in LAB: a-b Projection";
                    result.XAxisLabel = "a";
                    result.YAxisLabel = "b";
                    break;

                case ImageColorProjection2DType.Lab_LA:
                    result.Title = "Image Colors in LAB: L-a Projection";
                    result.XAxisLabel = "L";
                    result.YAxisLabel = "a";
                    break;

                case ImageColorProjection2DType.Lab_LB:
                    result.Title = "Image Colors in LAB: L-b Projection";
                    result.XAxisLabel = "L";
                    result.YAxisLabel = "b";
                    break;

                case ImageColorProjection2DType.YCbCr_CbCr:
                    result.Title = "Image Colors in YCbCr: Cb-Cr Projection";
                    result.XAxisLabel = "Cb";
                    result.YAxisLabel = "Cr";
                    break;

                case ImageColorProjection2DType.YCbCr_YCb:
                    result.Title = "Image Colors in YCbCr: Y-Cb Projection";
                    result.XAxisLabel = "Y";
                    result.YAxisLabel = "Cb";
                    break;

                case ImageColorProjection2DType.YCbCr_YCr:
                    result.Title = "Image Colors in YCbCr: Y-Cr Projection";
                    result.XAxisLabel = "Y";
                    result.YAxisLabel = "Cr";
                    break;

                case ImageColorProjection2DType.Yuv_UV:
                    result.Title = "Image Colors in YUV: U-V Projection";
                    result.XAxisLabel = "U";
                    result.YAxisLabel = "V";
                    break;

                case ImageColorProjection2DType.Yuv_YU:
                    result.Title = "Image Colors in YUV: Y-U Projection";
                    result.XAxisLabel = "Y";
                    result.YAxisLabel = "U";
                    break;

                case ImageColorProjection2DType.Yuv_YV:
                    result.Title = "Image Colors in YUV: Y-V Projection";
                    result.XAxisLabel = "Y";
                    result.YAxisLabel = "V";
                    break;

                case ImageColorProjection2DType.Cmyk_CM:
                    result.Title = "Image Colors in CMYK: C-M Projection";
                    result.XAxisLabel = "C";
                    result.YAxisLabel = "M";
                    break;

                case ImageColorProjection2DType.Cmyk_CK:
                    result.Title = "Image Colors in CMYK: C-K Projection";
                    result.XAxisLabel = "C";
                    result.YAxisLabel = "K";
                    break;

                case ImageColorProjection2DType.Cmyk_YK:
                    result.Title = "Image Colors in CMYK: Y-K Projection";
                    result.XAxisLabel = "Y";
                    result.YAxisLabel = "K";
                    break;
            }

            return result;
        }

        private int CalculateSamplingStep(
            int width,
            int height,
            int maxSampleCount)
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

        private ImageColorPoint2D ProjectColor(byte r, byte g, byte b, Color displayColor, ImageColorProjection2DType projection)
        {
            switch (projection)
            {
                case ImageColorProjection2DType.Rgb_RG:
                    return CreatePoint(r / 255.0, g / 255.0, displayColor);

                case ImageColorProjection2DType.Rgb_RB:
                    return CreatePoint(r / 255.0, b / 255.0, displayColor);

                case ImageColorProjection2DType.Rgb_GB:
                    return CreatePoint(g / 255.0, b / 255.0, displayColor);
            }

            if (IsHsvProjection(projection))
            {
                double h;
                double s;
                double v;

                RgbToHsv(r, g, b, out h, out s, out v);

                if (projection == ImageColorProjection2DType.Hsv_HS)
                    return CreatePoint(h / 360.0, s, displayColor);

                if (projection == ImageColorProjection2DType.Hsv_HV)
                    return CreatePoint(h / 360.0, v, displayColor);

                return CreatePoint(s, v, displayColor);
            }

            if (IsLabProjection(projection))
            {
                double l;
                double a;
                double labB;

                RgbToLab(r, g, b, out l, out a, out labB);

                double lNorm = l / 100.0;
                double aNorm = (a + 128.0) / 255.0;
                double bNorm = (labB + 128.0) / 255.0;

                if (projection == ImageColorProjection2DType.Lab_AB)
                    return CreatePoint(aNorm, bNorm, displayColor);

                if (projection == ImageColorProjection2DType.Lab_LA)
                    return CreatePoint(lNorm, aNorm, displayColor);

                return CreatePoint(lNorm, bNorm, displayColor);
            }

            if (IsYCbCrProjection(projection))
            {
                double y;
                double cb;
                double cr;

                RgbToYCbCr(r, g, b, out y, out cb, out cr);

                if (projection == ImageColorProjection2DType.YCbCr_CbCr)
                    return CreatePoint(cb / 255.0, cr / 255.0, displayColor);

                if (projection == ImageColorProjection2DType.YCbCr_YCb)
                    return CreatePoint(y / 255.0, cb / 255.0, displayColor);

                return CreatePoint(y / 255.0, cr / 255.0, displayColor);
            }

            if (IsYuvProjection(projection))
            {
                double y;
                double u;
                double v;

                RgbToYuv(r, g, b, out y, out u, out v);

                if (projection == ImageColorProjection2DType.Yuv_UV)
                    return CreatePoint(u / 255.0, v / 255.0, displayColor);

                if (projection == ImageColorProjection2DType.Yuv_YU)
                    return CreatePoint(y / 255.0, u / 255.0, displayColor);

                return CreatePoint(y / 255.0, v / 255.0, displayColor);
            }

            if (IsCmykProjection(projection))
            {
                double c;
                double m;
                double yy;
                double k;

                RgbToCmyk(r, g, b, out c, out m, out yy, out k);

                if (projection == ImageColorProjection2DType.Cmyk_CM)
                    return CreatePoint(c, m, displayColor);

                if (projection == ImageColorProjection2DType.Cmyk_CK)
                    return CreatePoint(c, k, displayColor);

                return CreatePoint(yy, k, displayColor);
            }

            return CreatePoint(0, 0, displayColor);
        }

        private ImageColorPoint2D CreatePoint(
            double x,
            double y,
            Color displayColor)
        {
            x = Clamp01(x);
            y = Clamp01(y);

            /*
             * في الرسم y=0 يعني أعلى الشاشة.
             * لكن رياضيًا نريد y=0 في الأسفل و y=1 في الأعلى.
             * لذلك نقلب y هنا.
             */
            return new ImageColorPoint2D(
                (float)x,
                (float)(1.0 - y),
                displayColor);
        }

        private bool IsHsvProjection(ImageColorProjection2DType projection)
        {
            return projection == ImageColorProjection2DType.Hsv_HS ||
                   projection == ImageColorProjection2DType.Hsv_HV ||
                   projection == ImageColorProjection2DType.Hsv_SV;
        }

        private bool IsLabProjection(ImageColorProjection2DType projection)
        {
            return projection == ImageColorProjection2DType.Lab_AB ||
                   projection == ImageColorProjection2DType.Lab_LA ||
                   projection == ImageColorProjection2DType.Lab_LB;
        }

        private bool IsYCbCrProjection(ImageColorProjection2DType projection)
        {
            return projection == ImageColorProjection2DType.YCbCr_CbCr ||
                   projection == ImageColorProjection2DType.YCbCr_YCb ||
                   projection == ImageColorProjection2DType.YCbCr_YCr;
        }

        private bool IsYuvProjection(ImageColorProjection2DType projection)
        {
            return projection == ImageColorProjection2DType.Yuv_UV ||
                   projection == ImageColorProjection2DType.Yuv_YU ||
                   projection == ImageColorProjection2DType.Yuv_YV;
        }

        private bool IsCmykProjection(ImageColorProjection2DType projection)
        {
            return projection == ImageColorProjection2DType.Cmyk_CM ||
                   projection == ImageColorProjection2DType.Cmyk_CK ||
                   projection == ImageColorProjection2DType.Cmyk_YK;
        }

        private void RgbToHsv(
            byte rByte,
            byte gByte,
            byte bByte,
            out double h,
            out double s,
            out double v)
        {
            double r = rByte / 255.0;
            double g = gByte / 255.0;
            double b = bByte / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            if (delta == 0)
            {
                h = 0;
            }
            else if (max == r)
            {
                h = 60.0 * (((g - b) / delta) % 6.0);
            }
            else if (max == g)
            {
                h = 60.0 * (((b - r) / delta) + 2.0);
            }
            else
            {
                h = 60.0 * (((r - g) / delta) + 4.0);
            }

            if (h < 0)
                h += 360.0;

            s = max == 0 ? 0 : delta / max;
            v = max;
        }

        private void RgbToYCbCr(
            byte r,
            byte g,
            byte b,
            out double y,
            out double cb,
            out double cr)
        {
            y = 0.299 * r + 0.587 * g + 0.114 * b;
            cb = 128.0 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            cr = 128.0 + 0.5 * r - 0.418688 * g - 0.081312 * b;

            y = Clamp255(y);
            cb = Clamp255(cb);
            cr = Clamp255(cr);
        }

        private void RgbToYuv(
            byte r,
            byte g,
            byte b,
            out double y,
            out double u,
            out double v)
        {
            y = 0.299 * r + 0.587 * g + 0.114 * b;

            double rawU = -0.14713 * r - 0.28886 * g + 0.436 * b;
            double rawV = 0.615 * r - 0.51499 * g - 0.10001 * b;

            u = rawU + 128.0;
            v = rawV + 128.0;

            y = Clamp255(y);
            u = Clamp255(u);
            v = Clamp255(v);
        }

        private void RgbToCmyk(
            byte rByte,
            byte gByte,
            byte bByte,
            out double c,
            out double m,
            out double y,
            out double k)
        {
            double r = rByte / 255.0;
            double g = gByte / 255.0;
            double b = bByte / 255.0;

            k = 1.0 - Math.Max(r, Math.Max(g, b));

            c = 0.0;
            m = 0.0;
            y = 0.0;

            if (k < 1.0)
            {
                c = (1.0 - r - k) / (1.0 - k);
                m = (1.0 - g - k) / (1.0 - k);
                y = (1.0 - b - k) / (1.0 - k);
            }

            c = Clamp01(c);
            m = Clamp01(m);
            y = Clamp01(y);
            k = Clamp01(k);
        }

        private void RgbToLab(
            byte rByte,
            byte gByte,
            byte bByte,
            out double l,
            out double a,
            out double b)
        {
            double r = PivotRgb(rByte / 255.0);
            double g = PivotRgb(gByte / 255.0);
            double bb = PivotRgb(bByte / 255.0);

            double x = r * 0.4124564 + g * 0.3575761 + bb * 0.1804375;
            double y = r * 0.2126729 + g * 0.7151522 + bb * 0.0721750;
            double z = r * 0.0193339 + g * 0.1191920 + bb * 0.9503041;

            double xRef = 0.95047;
            double yRef = 1.00000;
            double zRef = 1.08883;

            double fx = PivotXyz(x / xRef);
            double fy = PivotXyz(y / yRef);
            double fz = PivotXyz(z / zRef);

            l = 116.0 * fy - 16.0;
            a = 500.0 * (fx - fy);
            b = 200.0 * (fy - fz);
        }

        private double PivotRgb(double value)
        {
            if (value > 0.04045)
                return Math.Pow((value + 0.055) / 1.055, 2.4);

            return value / 12.92;
        }

        private double PivotXyz(double value)
        {
            if (value > 0.008856)
                return Math.Pow(value, 1.0 / 3.0);

            return 7.787 * value + 16.0 / 116.0;
        }

        private double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 1.0)
                return 1.0;

            return value;
        }

        private double Clamp255(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 255.0)
                return 255.0;

            return value;
        }
    }
}
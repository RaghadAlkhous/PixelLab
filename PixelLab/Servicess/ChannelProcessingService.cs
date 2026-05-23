using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using PixelLab.Models;
using PixelLab.Utils;


namespace PixelLab.Servicess
{
    public class ChannelProcessingService
    {
        public ChannelProcessingResult Process(Bitmap sourceBitmap, ChannelProcessingSettings settings)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Stopwatch stopwatch = Stopwatch.StartNew();

            Bitmap resultBitmap;

            if (settings.ColorSpace == ColorSpaceType.CmykPreview)
                resultBitmap = ProcessCmyk(sourceBitmap, settings);
            else
                resultBitmap = ProcessWithEmgu(sourceBitmap, settings);

            stopwatch.Stop();

            ColorSpaceChannelInfo info = ColorSpaceChannelInfoProvider.GetInfo(settings.ColorSpace);

            string description = info.DisplayName + " channels - " + settings.ViewMode.ToString();

            return new ChannelProcessingResult(
                resultBitmap, description, stopwatch.ElapsedMilliseconds
            );
        }

        private Bitmap ProcessWithEmgu(Bitmap sourceBitmap, ChannelProcessingSettings settings)
        {
            using (Mat bgrSource = BitmapToBgrMat(sourceBitmap))
            using (Mat colorSpaceMat = ConvertBgrToColorSpace(bgrSource, settings.ColorSpace))
            using (VectorOfMat channels = new VectorOfMat())
            {
                CvInvoke.Split(colorSpaceMat, channels);

                ColorSpaceChannelInfo info = ColorSpaceChannelInfoProvider.GetInfo(settings.ColorSpace);

                int channelCount = info.ChannelCount;

                Mat[] adjustedChannels = new Mat[channelCount];

                for (int i = 0; i < channelCount; i++)
                {
                    Mat channel = channels[i];

                    byte neutralValue = GetNeutralChannelValue(settings.ColorSpace, i);

                    bool wrap = ShouldWrapChannel(settings.ColorSpace, i);

                    int wrapMax = GetWrapMaxValue(settings.ColorSpace, i);

                    adjustedChannels[i] = ApplyChannelEdit(
                        channel, IsChannelEnabled(settings, i), GetChannelOffset(settings, i), neutralValue,wrap , wrapMax
                    );
                }

                Bitmap output;

                if (settings.ViewMode == ChannelViewMode.SingleChannel)
                {
                    int selectedIndex = ClampSelectedChannelIndex(settings.SelectedChannelIndex, channelCount);

                    using (Mat selectedBgr = ConvertSingleChannelToBgr(adjustedChannels[selectedIndex]))
                    {
                        output = Emgu.CV.BitmapExtension.ToBitmap(selectedBgr);
                    }
                }
                else
                {
                    using (VectorOfMat mergeVector = new VectorOfMat())
                    using (Mat adjustedColorSpaceMat = new Mat())
                    using (Mat bgrResult = new Mat())
                    {
                        for (int i = 0; i < channelCount; i++)
                            mergeVector.Push(adjustedChannels[i]);

                        CvInvoke.Merge(mergeVector, adjustedColorSpaceMat);

                        ConvertColorSpaceToBgr(adjustedColorSpaceMat, bgrResult, settings.ColorSpace);

                        output = Emgu.CV.BitmapExtension.ToBitmap(bgrResult);
                    }
                }

                for (int i = 0; i < adjustedChannels.Length; i++)
                {
                    if (adjustedChannels[i] != null)
                        adjustedChannels[i].Dispose();
                }

                return output;
            }
        }

        private Mat ApplyChannelEdit(
            Mat channel, bool enabled, int offset, byte neutralValue, bool wrap, int wrapMax
        ) {
            using (Image<Gray, byte> channelImage = channel.ToImage<Gray, byte>())
            {
                byte[,,] data = channelImage.Data;

                int height = channelImage.Height;
                int width = channelImage.Width;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (!enabled)
                        {
                            data[y, x, 0] = neutralValue;
                            continue;
                        }

                        int value = data[y, x, 0] + offset;

                        if (wrap)
                        {
                            value = Mod(value, wrapMax);
                        }
                        else
                        {
                            value = ClampToByte(value);
                        }

                        data[y, x, 0] = (byte)value;
                    }
                }

                return channelImage.Mat.Clone();
            }
        }

        private Mat ConvertSingleChannelToBgr(Mat channel)
        {
            Mat bgr = new Mat();

            CvInvoke.CvtColor(channel, bgr, ColorConversion.Gray2Bgr);

            return bgr;
        }

        private Mat ConvertBgrToColorSpace(Mat bgrSource, ColorSpaceType colorSpace)
        {
            Mat result = new Mat();

            switch (colorSpace)
            {
                case ColorSpaceType.RgbOriginal:
                    CvInvoke.CvtColor(bgrSource, result, ColorConversion.Bgr2Rgb);
                    return result;

                case ColorSpaceType.Grayscale:
                    CvInvoke.CvtColor(bgrSource, result, ColorConversion.Bgr2Gray);
                    return result;

                case ColorSpaceType.Hsv:
                    CvInvoke.CvtColor(bgrSource, result, ColorConversion.Bgr2Hsv);
                    return result;

                case ColorSpaceType.Lab:
                    CvInvoke.CvtColor(bgrSource, result,ColorConversion.Bgr2Lab);
                    return result;

                case ColorSpaceType.YCbCr:
                    CvInvoke.CvtColor(bgrSource, result, ColorConversion.Bgr2YCrCb);
                    return result;

                case ColorSpaceType.Yuv:
                    CvInvoke.CvtColor(bgrSource, result, ColorConversion.Bgr2Yuv);
                    return result;

                default:
                    result.Dispose();throw new NotSupportedException("Unsupported color space: " + colorSpace);
            }
        }

        private void ConvertColorSpaceToBgr(Mat colorSpaceMat, Mat bgrResult, ColorSpaceType colorSpace)
        {
            switch (colorSpace)
            {
                case ColorSpaceType.RgbOriginal:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.Rgb2Bgr);
                    return;

                case ColorSpaceType.Grayscale:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.Gray2Bgr);
                    return;

                case ColorSpaceType.Hsv:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.Hsv2Bgr);
                    return;

                case ColorSpaceType.Lab:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.Lab2Bgr);
                    return;

                case ColorSpaceType.YCbCr:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.YCrCb2Bgr);
                    return;

                case ColorSpaceType.Yuv:
                    CvInvoke.CvtColor(colorSpaceMat, bgrResult, ColorConversion.Yuv2Bgr);
                    return;

                default:
                    throw new NotSupportedException("Unsupported color space: " + colorSpace);
            }
        }

        private byte GetNeutralChannelValue(ColorSpaceType colorSpace, int channelIndex)
        {
            /*
             * القيم المحايدة عند تعطيل قناة:
             *
             * RGB:
             * تعطيل قناة يعني 0.
             *
             * HSV:
             * H = 0
             * S = 0 يجعل اللون رماديًا.
             * V = 0 يجعل البكسل أسود.
             *
             * LAB:
             * L = 0: يعني إضاءة معدومة
             * a و b محايدهما في OpenCV 8-bit هو 128.
             *
             * YCrCb / YUV:
             * Y = 0 للإضاءة.
             * Cr/Cb/U/V محايدها 128.
             */
            switch (colorSpace)
            {
                case ColorSpaceType.RgbOriginal:
                    return 0;

                case ColorSpaceType.Grayscale:
                    return 0;

                case ColorSpaceType.Hsv:
                    return 0;

                case ColorSpaceType.Lab:
                    if (channelIndex == 0)
                        return 0;

                    return 128;

                case ColorSpaceType.YCbCr:
                    if (channelIndex == 0)
                        return 0;

                    return 128;

                case ColorSpaceType.Yuv:
                    if (channelIndex == 0)
                        return 0;

                    return 128;

                default:
                    return 0;
            }
        }

        private bool ShouldWrapChannel(ColorSpaceType colorSpace, int channelIndex)
        {
            /*
             * Hue قناة دائرية.
             * في OpenCV HSV 8-bit:
             * H عادة من 0 إلى 179.
             * لذلك تعديل Hue يجب أن يلتف بدل أن يتوقف عند 0 أو 179.
             */
            return colorSpace == ColorSpaceType.Hsv && channelIndex == 0;
        }

        private int GetWrapMaxValue(ColorSpaceType colorSpace, int channelIndex)
        {
            if (colorSpace == ColorSpaceType.Hsv && channelIndex == 0)
                return 180;

            return 256;
        }

        private bool IsChannelEnabled(ChannelProcessingSettings settings, int index)
        {
            if (settings.ChannelEnabled == null)
                return true;

            if (index < 0 || index >= settings.ChannelEnabled.Length)
                return true;

            return settings.ChannelEnabled[index];
        }

        private int GetChannelOffset(ChannelProcessingSettings settings, int index)
        {
            if (settings.ChannelOffsets == null)
                return 0;

            if (index < 0 || index >= settings.ChannelOffsets.Length)
                return 0;

            return settings.ChannelOffsets[index];
        }

        private int ClampSelectedChannelIndex(int selectedIndex, int channelCount)
        {
            if (selectedIndex < 0)
                return 0;

            if (selectedIndex >= channelCount)
                return channelCount - 1;

            return selectedIndex;
        }

        private int ClampToByte(int value)
        {
            if (value < 0)
                return 0;

            if (value > 255)
                return 255;

            return value;
        }

        private int Mod(int value, int modulo)
        {
            int result = value % modulo;

            if (result < 0)
                result += modulo;

            return result;
        }

        private Mat BitmapToBgrMat(Bitmap sourceBitmap)
        {
            using (Bitmap normalizedBitmap = Helpers.Ensure24bppRgb(sourceBitmap))
            {
                Mat mat = Emgu.CV.BitmapExtension.ToMat(normalizedBitmap);

                if (mat.NumberOfChannels == 3)
                    return mat;

                if (mat.NumberOfChannels == 4)
                {
                    Mat bgr = new Mat();

                    CvInvoke.CvtColor(mat, bgr, ColorConversion.Bgra2Bgr);

                    mat.Dispose();

                    return bgr;
                }

                if (mat.NumberOfChannels == 1)
                {
                    Mat bgr = new Mat();

                    CvInvoke.CvtColor(mat, bgr, ColorConversion.Gray2Bgr);

                    mat.Dispose();

                    return bgr;
                }

                mat.Dispose();

                throw new NotSupportedException("Unsupported number of channels in bitmap.");
            }
        }

        private Bitmap ProcessCmyk(Bitmap sourceBitmap, ChannelProcessingSettings settings)
        {
            using (Bitmap source24 = Helpers.Ensure24bppRgb(sourceBitmap))
            {
                Bitmap result = new Bitmap(source24.Width, source24.Height, PixelFormat.Format24bppRgb);

                Rectangle rect = new Rectangle(0, 0, source24.Width, source24.Height);

                BitmapData srcData = source24.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                BitmapData dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                try
                {
                    int srcBytesCount = Math.Abs(srcData.Stride) * source24.Height;

                    int dstBytesCount = Math.Abs(dstData.Stride) * result.Height;

                    byte[] srcBytes = new byte[srcBytesCount];
                    byte[] dstBytes = new byte[dstBytesCount];

                    Marshal.Copy(srcData.Scan0, srcBytes, 0, srcBytesCount);

                    int selectedChannel = ClampSelectedChannelIndex(settings.SelectedChannelIndex, 4);

                    for (int y = 0; y < source24.Height; y++)
                    {
                        int srcRow = y * srcData.Stride;
                        int dstRow = y * dstData.Stride;

                        for (int x = 0; x < source24.Width; x++)
                        {
                            int srcIndex = srcRow + x * 3;
                            int dstIndex = dstRow + x * 3;

                            byte bByte = srcBytes[srcIndex + 0];
                            byte gByte = srcBytes[srcIndex + 1];
                            byte rByte = srcBytes[srcIndex + 2];

                            double r = rByte / 255.0;
                            double g = gByte / 255.0;
                            double b = bByte / 255.0;

                            double k = 1.0 - Math.Max(r, Math.Max(g, b));

                            double c  = 0.0;
                            double m  = 0.0;
                            double yy = 0.0;

                            if (k < 1.0)
                            {
                                c  = (1.0 - r - k) / (1.0 - k);
                                m  = (1.0 - g - k) / (1.0 - k);
                                yy = (1.0 - b - k) / (1.0 - k);
                            }

                            byte[] cmyk = new byte[4];

                            cmyk[0] = Helpers.ToByte(c * 255.0);
                            cmyk[1] = Helpers.ToByte(m * 255.0);
                            cmyk[2] = Helpers.ToByte(yy * 255.0);
                            cmyk[3] = Helpers.ToByte(k * 255.0);

                            for (int i = 0; i < 4; i++)
                            {
                                if (!IsChannelEnabled(settings, i))
                                {
                                    cmyk[i] = 0;
                                }
                                else
                                {
                                    int value = cmyk[i] + GetChannelOffset(settings, i);

                                    cmyk[i] = (byte)ClampToByte(value);
                                }
                            }

                            if (settings.ViewMode == ChannelViewMode.SingleChannel)
                            {
                                byte gray = cmyk[selectedChannel];

                                dstBytes[dstIndex + 0] = gray;
                                dstBytes[dstIndex + 1] = gray;
                                dstBytes[dstIndex + 2] = gray;
                            }
                            else
                            {
                                double cc  = cmyk[0] / 255.0;
                                double mm  = cmyk[1] / 255.0;
                                double yyy = cmyk[2] / 255.0;
                                double kk  = cmyk[3] / 255.0;

                                double rr = (1.0 - cc)  * (1.0 - kk);
                                double gg = (1.0 - mm)  * (1.0 - kk);
                                double bb = (1.0 - yyy) * (1.0 - kk);

                                dstBytes[dstIndex + 0] = Helpers.ToByte(bb * 255.0);
                                dstBytes[dstIndex + 1] = Helpers.ToByte(gg * 255.0);
                                dstBytes[dstIndex + 2] = Helpers.ToByte(rr * 255.0);
                            }
                        }
                    }

                    Marshal.Copy(dstBytes, 0, dstData.Scan0, dstBytesCount);
                }
                finally
                {
                    source24.UnlockBits(srcData);
                    result.UnlockBits(dstData);
                }

                return result;
            }
        }
    }
}
